using DevMemory.Application;
using DevMemory.Application.Abstractions;
using DevMemory.Application.Ai.Indexing;
using DevMemory.Application.Ai.Search;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;
using DevMemory.Cli.CommandLine;
using DevMemory.Infrastructure;
using DevMemory.Infrastructure.Ai;
using DevMemory.Infrastructure.Ai.Factories;

namespace DevMemory.Cli.Commands.Ai;

public sealed class RelatedCommandHandler : ICommandHandler
{
    private const int DefaultLimit = 5;
    private const int MaxSearchLimit = 50;

    private readonly MemoryService _memoryService;
    private readonly Func<AiRuntimeOptions> _optionsFactory;
    private readonly Func<AiRuntimeOptions, IEmbeddingService?> _embeddingServiceFactory;
    private readonly Func<AiRuntimeOptions, IVectorMemoryStore?> _vectorMemoryStoreFactory;
    private readonly Func<IEmbeddingService, IVectorMemoryStore, MemorySemanticSearchService> _semanticSearchServiceFactory;

    public RelatedCommandHandler(MemoryService memoryService)
        : this(
            memoryService,
            AiRuntimeOptionsProvider.GetOptions,
            EmbeddingServiceFactory.Create,
            VectorMemoryStoreFactory.Create,
            static (embeddingService, vectorMemoryStore) =>
                new MemorySemanticSearchService(embeddingService, vectorMemoryStore))
    {
    }

    public RelatedCommandHandler(
        MemoryService memoryService,
        Func<AiRuntimeOptions> optionsFactory,
        Func<AiRuntimeOptions, IEmbeddingService?> embeddingServiceFactory,
        Func<AiRuntimeOptions, IVectorMemoryStore?> vectorMemoryStoreFactory,
        Func<IEmbeddingService, IVectorMemoryStore, MemorySemanticSearchService> semanticSearchServiceFactory)
    {
        _memoryService = memoryService ?? throw new ArgumentNullException(nameof(memoryService));
        _optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
        _embeddingServiceFactory = embeddingServiceFactory
            ?? throw new ArgumentNullException(nameof(embeddingServiceFactory));
        _vectorMemoryStoreFactory = vectorMemoryStoreFactory
            ?? throw new ArgumentNullException(nameof(vectorMemoryStoreFactory));
        _semanticSearchServiceFactory = semanticSearchServiceFactory
            ?? throw new ArgumentNullException(nameof(semanticSearchServiceFactory));
    }

    public string Name => "related";

    public int Execute(string[] args)
    {
        RelatedCommandRequest request;

        try
        {
            request = ParseRequest(args);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine();

            PrintUsageToError();

            return CliExitCodes.InvalidCommand;
        }

        var sourceMemory = _memoryService.GetById(request.MemoryId);

        if (sourceMemory is null)
        {
            Console.Error.WriteLine($"Memory not found: {request.MemoryId}");

            return CliExitCodes.Failure;
        }

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
            var sourceDocument = VectorMemoryDocumentBuilder
                .BuildFromMemories([sourceMemory])
                .Single();

            var semanticSearchService = _semanticSearchServiceFactory(
                embeddingService,
                vectorMemoryStore);

            var searchLimit = Math.Min(request.Limit + 1, MaxSearchLimit);

            var results = semanticSearchService
                .SearchAsync(
                    sourceDocument.Text,
                    ResolveEmbeddingModel(options.Embedding),
                    searchLimit,
                    CancellationToken.None)
                .GetAwaiter()
                .GetResult()
                .Where(result => result.MemoryId != request.MemoryId)
                .Take(request.Limit)
                .ToList();

            PrintResults(request, options, sourceDocument, results);

            return CliExitCodes.Success;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            AiRuntimeErrorPrinter.PrintFailure("Related memories search", ex);

            return CliExitCodes.Failure;
        }
        finally
        {
            DisposeIfRequired(embeddingService);
            DisposeIfRequired(vectorMemoryStore);
        }
    }

    /// <summary>
    /// Parses related command-line arguments.
    /// </summary>
    private static RelatedCommandRequest ParseRequest(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Memory id is required.");
        }

        if (!Guid.TryParse(args[1], out var memoryId))
        {
            throw new ArgumentException("Invalid memory id. The memory id must be a valid GUID.");
        }

        var limit = DefaultLimit;
        var showPreview = false;

        for (var index = 2; index < args.Length; index++)
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

            if (value.Equals("--show-preview", StringComparison.OrdinalIgnoreCase))
            {
                showPreview = true;
                continue;
            }

            throw new ArgumentException($"Unknown related option: {value}");
        }

        return new RelatedCommandRequest(memoryId, limit, showPreview);
    }

    /// <summary>
    /// Prints related memory search results.
    /// </summary>
    private static void PrintResults(
        RelatedCommandRequest request,
        AiRuntimeOptions options,
        VectorMemoryDocument sourceDocument,
        List<VectorMemorySearchResult> results)
    {
        Console.WriteLine("Related memories");
        Console.WriteLine("----------------");
        Console.WriteLine();
        Console.WriteLine("Source:");
        Console.WriteLine($"Title: {FormatOptional(sourceDocument.Title)}");
        Console.WriteLine($"MemoryId: {sourceDocument.MemoryId:D}");
        Console.WriteLine($"Project: {FormatOptional(sourceDocument.Project)}");
        Console.WriteLine($"Area: {FormatOptional(sourceDocument.Area)}");
        Console.WriteLine();
        Console.WriteLine($"Limit: {request.Limit}");
        Console.WriteLine($"Embedding provider: {options.Embedding.Provider}");
        Console.WriteLine($"Embedding model: {ResolveEmbeddingModel(options.Embedding)}");
        Console.WriteLine($"Vector store: {options.VectorStore.Provider}");
        Console.WriteLine($"Results: {results.Count}");
        Console.WriteLine();

        if (results.Count == 0)
        {
            Console.WriteLine("No related indexed memories were found.");
            Console.WriteLine();
            Console.WriteLine("Run:");
            Console.WriteLine("  devmemory index");
            Console.WriteLine();
            Console.WriteLine("or inspect indexed documents with:");
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

            if (request.ShowPreview)
            {
                Console.WriteLine("   Preview:");
                PrintIndentedPreview(result.Text, indentation: "     ");
            }

            Console.WriteLine();

            position++;
        }
    }

    /// <summary>
    /// Prints a bounded text preview using the provided indentation.
    /// </summary>
    private static void PrintIndentedPreview(string text, string indentation)
    {
        const int maxPreviewLength = 500;

        var preview = string.IsNullOrWhiteSpace(text)
            ? "-"
            : text.Trim();

        if (preview.Length > maxPreviewLength)
        {
            preview = string.Concat(preview.AsSpan(0, maxPreviewLength), "...");
        }

        using var reader = new StringReader(preview);

        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            Console.WriteLine($"{indentation}{line}");
        }
    }

    /// <summary>
    /// Prints command usage to the error stream.
    /// </summary>
    private static void PrintUsageToError()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  devmemory related <memory-id> [--limit <number>] [--show-preview]");
    }

    /// <summary>
    /// Prints a user-friendly message when semantic search is not configured.
    /// </summary>
    private static void PrintSemanticSearchNotConfigured()
    {
        Console.Error.WriteLine("Related memories search is not configured.");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Configure both an embedding provider and a vector store before searching related memories.");
        Console.Error.WriteLine("Example:");
        Console.Error.WriteLine("  devmemory config set embedding-provider ollama");
        Console.Error.WriteLine("  devmemory config set vector-store qdrant");
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

    private sealed record RelatedCommandRequest(
        Guid MemoryId,
        int Limit,
        bool ShowPreview);
}
