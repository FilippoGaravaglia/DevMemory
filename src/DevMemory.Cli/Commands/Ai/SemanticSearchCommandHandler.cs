using DevMemory.Application.Abstractions;
using DevMemory.Application.Ai.Search;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;
using DevMemory.Cli.CommandLine;
using DevMemory.Infrastructure;
using DevMemory.Infrastructure.Ai;
using DevMemory.Infrastructure.Ai.Factories;

namespace DevMemory.Cli.Commands.Ai;

public sealed class SemanticSearchCommandHandler : ICommandHandler
{
    private const int DefaultLimit = 5;

    private readonly Func<AiRuntimeOptions> _optionsFactory;
    private readonly Func<AiRuntimeOptions, IEmbeddingService?> _embeddingServiceFactory;
    private readonly Func<AiRuntimeOptions, IVectorMemoryStore?> _vectorMemoryStoreFactory;
    private readonly Func<IEmbeddingService, IVectorMemoryStore, MemorySemanticSearchService> _semanticSearchServiceFactory;

    public SemanticSearchCommandHandler()
        : this(
            AiRuntimeOptionsProvider.GetOptions,
            EmbeddingServiceFactory.Create,
            VectorMemoryStoreFactory.Create,
            static (embeddingService, vectorMemoryStore) =>
                new MemorySemanticSearchService(embeddingService, vectorMemoryStore))
    {
    }

    public SemanticSearchCommandHandler(
        Func<AiRuntimeOptions> optionsFactory,
        Func<AiRuntimeOptions, IEmbeddingService?> embeddingServiceFactory,
        Func<AiRuntimeOptions, IVectorMemoryStore?> vectorMemoryStoreFactory,
        Func<IEmbeddingService, IVectorMemoryStore, MemorySemanticSearchService> semanticSearchServiceFactory)
    {
        _optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
        _embeddingServiceFactory = embeddingServiceFactory
            ?? throw new ArgumentNullException(nameof(embeddingServiceFactory));
        _vectorMemoryStoreFactory = vectorMemoryStoreFactory
            ?? throw new ArgumentNullException(nameof(vectorMemoryStoreFactory));
        _semanticSearchServiceFactory = semanticSearchServiceFactory
            ?? throw new ArgumentNullException(nameof(semanticSearchServiceFactory));
    }

    public string Name => "semantic-search";

    public int Execute(string[] args)
    {
        var request = ParseRequest(args);
        var options = _optionsFactory();

        if (!options.IsSemanticSearchEnabled)
        {
            PrintSemanticSearchNotConfigured();

            return CliExitCodes.Failure;
        }

        var embeddingService = _embeddingServiceFactory(options);

        if (embeddingService is null)
        {
            PrintEmbeddingAdapterNotAvailable(options.Embedding.Provider);

            return CliExitCodes.Failure;
        }

        var vectorMemoryStore = _vectorMemoryStoreFactory(options);

        if (vectorMemoryStore is null)
        {
            DisposeIfRequired(embeddingService);
            PrintVectorStoreAdapterNotAvailable(options.VectorStore.Provider);

            return CliExitCodes.Failure;
        }

        try
        {
            var semanticSearchService = _semanticSearchServiceFactory(
                (IEmbeddingService)embeddingService,
                (IVectorMemoryStore)vectorMemoryStore);

            var results = semanticSearchService
                .SearchAsync(
                    request.Query,
                    ResolveEmbeddingModel(options.Embedding),
                    request.Limit,
                    CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            PrintResults(request, options, results);

            return CliExitCodes.Success;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            AiRuntimeErrorPrinter.PrintFailure("Semantic search", ex);

            return CliExitCodes.Failure;
        }
        finally
        {
            DisposeIfRequired(embeddingService);
            DisposeIfRequired(vectorMemoryStore);
        }
    }

    /// <summary>
    /// Parses the semantic search command-line request.
    /// </summary>
    private static SemanticSearchCliRequest ParseRequest(string[] args)
    {
        var queryParts = new List<string>();
        var limit = DefaultLimit;

        for (var index = 1; index < args.Length; index++)
        {
            var value = args[index];

            if (value.Equals("--limit", StringComparison.OrdinalIgnoreCase))
            {
                var limitValue = CommandOptions.ReadOptionValue(args, ref index, "--limit");

                if (!int.TryParse(limitValue, out limit) || limit <= 0)
                {
                    throw new ArgumentException("Option --limit must be a positive integer.");
                }

                continue;
            }

            queryParts.Add(value);
        }

        var query = string.Join(' ', queryParts).Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Usage: devmemory semantic-search <query> [--limit <number>]");
        }

        return new SemanticSearchCliRequest(query, limit);
    }

    /// <summary>
    /// Prints semantic search results.
    /// </summary>
    private static void PrintResults(
        SemanticSearchCliRequest request,
        AiRuntimeOptions options,
        IReadOnlyCollection<VectorMemorySearchResult> results)
    {
        Console.WriteLine("DevMemory semantic search");
        Console.WriteLine("-------------------------");
        Console.WriteLine();
        Console.WriteLine($"Query: {request.Query}");
        Console.WriteLine($"Limit: {request.Limit}");
        Console.WriteLine($"Embedding provider: {options.Embedding.Provider}");
        Console.WriteLine($"Embedding model: {ResolveEmbeddingModel(options.Embedding)}");
        Console.WriteLine($"Vector store: {options.VectorStore.Provider}");
        Console.WriteLine($"Results: {results.Count}");
        Console.WriteLine();

        if (results.Count == 0)
        {
            Console.WriteLine("No indexed memories were found for this query.");
            Console.WriteLine();
            Console.WriteLine("Run:");
            Console.WriteLine("  devmemory index");
            Console.WriteLine();
            Console.WriteLine("or inspect what would be indexed with:");
            Console.WriteLine("  devmemory index --dry-run");

            return;
        }

        var position = 1;

        foreach (var result in results)
        {
            Console.WriteLine($"{position}. {FormatOptional(result.Title)}");
            Console.WriteLine($"   MemoryId: {result.MemoryId:D}");
            Console.WriteLine($"   Project: {FormatOptional(result.Project)}");
            Console.WriteLine($"   Area: {FormatOptional(result.Area)}");
            Console.WriteLine($"   Score: {result.Score}");
            Console.WriteLine();

            position++;
        }
    }

    /// <summary>
    /// Prints a user-friendly message when semantic search is not configured.
    /// </summary>
    private static void PrintSemanticSearchNotConfigured()
    {
        Console.Error.WriteLine("Semantic search is not configured.");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Configure both an embedding provider and a vector store before searching.");
        Console.Error.WriteLine("Example:");
        Console.Error.WriteLine("  DEVMEMORY_EMBEDDING_PROVIDER=ollama DEVMEMORY_VECTOR_STORE=qdrant devmemory ai-status");
    }

    /// <summary>
    /// Prints a user-friendly message when the selected embedding provider has no adapter.
    /// </summary>
    private static void PrintEmbeddingAdapterNotAvailable(string provider)
    {
        Console.Error.WriteLine($"Embedding provider '{provider}' is configured, but no adapter is available yet.");
        Console.Error.WriteLine("Currently implemented embedding providers: ollama");
    }

    /// <summary>
    /// Prints a user-friendly message when the selected vector store has no adapter.
    /// </summary>
    private static void PrintVectorStoreAdapterNotAvailable(string provider)
    {
        Console.Error.WriteLine($"Vector store '{provider}' is configured, but no adapter is available yet.");
        Console.Error.WriteLine("Currently implemented vector stores: qdrant");
    }

    /// <summary>
    /// Resolves the configured embedding model for the selected provider.
    /// </summary>
    private static string ResolveEmbeddingModel(EmbeddingProviderOptions options)
    {
        if (options.IsOllama)
        {
            return FormatOptional(options.OllamaEmbeddingModel);
        }

        if (options.IsOpenAi)
        {
            return FormatOptional(options.OpenAiEmbeddingModel);
        }

        if (options.IsGemini)
        {
            return FormatOptional(options.GeminiEmbeddingModel);
        }

        return "-";
    }

    /// <summary>
    /// Formats an optional string value for CLI output.
    /// </summary>
    private static string FormatOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    /// <summary>
    /// Disposes an object when it implements IDisposable.
    /// </summary>
    private static void DisposeIfRequired(object? instance)
    {
        if (instance is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private sealed record SemanticSearchCliRequest(string Query, int Limit);
}
