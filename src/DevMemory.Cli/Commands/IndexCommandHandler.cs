using DevMemory.Application.Abstractions.Ai;
using DevMemory.Application.Models.Ai;
using DevMemory.Cli.CommandLine;
using DevMemory.Infrastructure;
using DevMemory.Infrastructure.Ai;

namespace DevMemory.Cli.Commands;

public sealed class IndexCommandHandler : ICommandHandler
{
    private readonly Func<AiRuntimeOptions> _optionsFactory;
    private readonly Func<AiRuntimeOptions, IEmbeddingService?> _embeddingServiceFactory;
    private readonly Func<AiRuntimeOptions, IVectorMemoryStore?> _vectorMemoryStoreFactory;

    public IndexCommandHandler()
        : this(
            AiRuntimeOptionsProvider.GetOptions,
            EmbeddingServiceFactory.Create,
            VectorMemoryStoreFactory.Create)
    {
    }

    public IndexCommandHandler(
        Func<AiRuntimeOptions> optionsFactory,
        Func<AiRuntimeOptions, IEmbeddingService?> embeddingServiceFactory,
        Func<AiRuntimeOptions, IVectorMemoryStore?> vectorMemoryStoreFactory)
    {
        _optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
        _embeddingServiceFactory = embeddingServiceFactory
            ?? throw new ArgumentNullException(nameof(embeddingServiceFactory));
        _vectorMemoryStoreFactory = vectorMemoryStoreFactory
            ?? throw new ArgumentNullException(nameof(vectorMemoryStoreFactory));
    }

    public string Name => "index";

    public int Execute(string[] args)
    {
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
            Console.WriteLine("DevMemory vector index");
            Console.WriteLine("----------------------");
            Console.WriteLine();
            Console.WriteLine($"Embedding provider: {options.Embedding.Provider}");
            Console.WriteLine($"Embedding model: {ResolveEmbeddingModel(options.Embedding)}");
            Console.WriteLine($"Vector store: {options.VectorStore.Provider}");
            Console.WriteLine($"Qdrant endpoint: {FormatOptional(options.VectorStore.QdrantEndpoint)}");
            Console.WriteLine($"Qdrant collection: {FormatOptional(options.VectorStore.QdrantCollection)}");
            Console.WriteLine();
            Console.WriteLine("Vector indexing pipeline is configured.");
            Console.WriteLine("Memory indexing execution will be added in the next step.");

            return CliExitCodes.Success;
        }
        finally
        {
            DisposeIfRequired(embeddingService);
            DisposeIfRequired(vectorMemoryStore);
        }
    }

    /// <summary>
    /// Prints a user-friendly message when semantic search is not configured.
    /// </summary>
    private static void PrintSemanticSearchNotConfigured()
    {
        Console.Error.WriteLine("Semantic search is not configured.");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Configure both an embedding provider and a vector store before indexing.");
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
}
