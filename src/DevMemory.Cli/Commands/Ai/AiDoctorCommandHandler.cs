using System.Net.Http.Json;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Cli.CommandLine;
using DevMemory.Infrastructure;

namespace DevMemory.Cli.Commands.Ai;

public sealed class AiDoctorCommandHandler : ICommandHandler
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(3);

    private readonly Func<AiRuntimeOptions> _optionsFactory;
    private readonly Func<HttpClient> _httpClientFactory;

    public AiDoctorCommandHandler()
        : this(AiRuntimeOptionsProvider.GetOptions, static () => new HttpClient())
    {
    }

    public AiDoctorCommandHandler(
        Func<AiRuntimeOptions> optionsFactory,
        Func<HttpClient> httpClientFactory)
    {
        _optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public string Name => "ai-doctor";

    public int Execute(string[] args)
    {
        var options = _optionsFactory();

        Console.WriteLine("DevMemory AI doctor");
        Console.WriteLine("-------------------");
        Console.WriteLine();

        var hasFailures = false;

        hasFailures |= !PrintChatConfiguration(options);
        Console.WriteLine();

        hasFailures |= !PrintEmbeddingConfiguration(options);
        Console.WriteLine();

        hasFailures |= !PrintVectorStoreConfiguration(options);
        Console.WriteLine();

        hasFailures |= !CheckOllamaConnectivity(options);
        Console.WriteLine();

        hasFailures |= !CheckQdrantConnectivity(options);
        Console.WriteLine();

        if (hasFailures)
        {
            Console.WriteLine("Result: configuration requires attention.");

            return CliExitCodes.Failure;
        }

        Console.WriteLine("Result: AI environment looks ready.");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Prints and validates the chat provider configuration.
    /// </summary>
    private static bool PrintChatConfiguration(AiRuntimeOptions options)
    {
        Console.WriteLine("Chat");
        Console.WriteLine("----");
        Console.WriteLine($"Provider: {options.Chat.Provider}");

        if (!options.IsChatEnabled)
        {
            Console.WriteLine("Status: not configured");
            Console.WriteLine("Hint: set DEVMEMORY_CHAT_PROVIDER=ollama for local chat.");

            return false;
        }

        if (options.Chat.IsOllama)
        {
            Console.WriteLine($"Endpoint: {FormatOptional(options.Chat.OllamaEndpoint)}");
            Console.WriteLine($"Model: {FormatOptional(options.Chat.OllamaChatModel)}");

            return !string.IsNullOrWhiteSpace(options.Chat.OllamaEndpoint)
                && !string.IsNullOrWhiteSpace(options.Chat.OllamaChatModel);
        }

        Console.WriteLine("Status: configured");
        Console.WriteLine("Connectivity check: not implemented for this chat provider yet.");

        return true;
    }

    /// <summary>
    /// Prints and validates the embedding provider configuration.
    /// </summary>
    private static bool PrintEmbeddingConfiguration(AiRuntimeOptions options)
    {
        Console.WriteLine("Embeddings");
        Console.WriteLine("----------");
        Console.WriteLine($"Provider: {options.Embedding.Provider}");

        if (!options.IsSemanticSearchEnabled)
        {
            Console.WriteLine("Status: not configured");
            Console.WriteLine("Hint: set DEVMEMORY_EMBEDDING_PROVIDER=ollama for local embeddings.");

            return false;
        }

        if (options.Embedding.IsOllama)
        {
            Console.WriteLine($"Endpoint: {FormatOptional(options.Embedding.OllamaEndpoint)}");
            Console.WriteLine($"Model: {FormatOptional(options.Embedding.OllamaEmbeddingModel)}");

            return !string.IsNullOrWhiteSpace(options.Embedding.OllamaEndpoint)
                && !string.IsNullOrWhiteSpace(options.Embedding.OllamaEmbeddingModel);
        }

        Console.WriteLine("Status: configured");
        Console.WriteLine("Connectivity check: not implemented for this embedding provider yet.");

        return true;
    }

    /// <summary>
    /// Prints and validates the vector store configuration.
    /// </summary>
    private static bool PrintVectorStoreConfiguration(AiRuntimeOptions options)
    {
        Console.WriteLine("Vector store");
        Console.WriteLine("------------");
        Console.WriteLine($"Provider: {options.VectorStore.Provider}");

        if (!options.VectorStore.IsQdrant)
        {
            Console.WriteLine("Status: not configured");
            Console.WriteLine("Hint: set DEVMEMORY_VECTOR_STORE=qdrant for local vector search.");

            return false;
        }

        Console.WriteLine($"Endpoint: {FormatOptional(options.VectorStore.QdrantEndpoint)}");
        Console.WriteLine($"Collection: {FormatOptional(options.VectorStore.QdrantCollection)}");

        return !string.IsNullOrWhiteSpace(options.VectorStore.QdrantEndpoint)
            && !string.IsNullOrWhiteSpace(options.VectorStore.QdrantCollection);
    }

    /// <summary>
    /// Checks Ollama connectivity when Ollama is configured.
    /// </summary>
    private bool CheckOllamaConnectivity(AiRuntimeOptions options)
    {
        if (!options.Chat.IsOllama && !options.Embedding.IsOllama)
        {
            return true;
        }

        var endpoint = options.Chat.IsOllama
            ? options.Chat.OllamaEndpoint
            : options.Embedding.OllamaEndpoint;

        Console.WriteLine("Ollama connectivity");
        Console.WriteLine("-------------------");

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            Console.WriteLine("Status: skipped");
            Console.WriteLine("Reason: Ollama endpoint is missing.");

            return false;
        }

        var uri = BuildUri(endpoint, "/api/tags");

        return CheckHttpEndpoint("Ollama", uri);
    }

    /// <summary>
    /// Checks Qdrant connectivity when Qdrant is configured.
    /// </summary>
    private bool CheckQdrantConnectivity(AiRuntimeOptions options)
    {
        if (!options.VectorStore.IsQdrant)
        {
            return true;
        }

        Console.WriteLine("Qdrant connectivity");
        Console.WriteLine("-------------------");

        if (string.IsNullOrWhiteSpace(options.VectorStore.QdrantEndpoint))
        {
            Console.WriteLine("Status: skipped");
            Console.WriteLine("Reason: Qdrant endpoint is missing.");

            return false;
        }

        var uri = BuildUri(options.VectorStore.QdrantEndpoint, "/collections");

        return CheckHttpEndpoint("Qdrant", uri);
    }

    /// <summary>
    /// Performs a lightweight HTTP GET health check.
    /// </summary>
    private bool CheckHttpEndpoint(string name, Uri uri)
    {
        using var httpClient = _httpClientFactory();
        httpClient.Timeout = DefaultTimeout;

        try
        {
            using var response = httpClient
                .GetAsync(uri, CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            Console.WriteLine($"Endpoint: {uri}");
            Console.WriteLine($"Status code: {(int)response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Status: reachable");

                return true;
            }

            Console.WriteLine("Status: not healthy");

            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            Console.WriteLine($"Endpoint: {uri}");
            Console.WriteLine("Status: unreachable");
            Console.WriteLine($"Reason: {ex.Message}");

            return false;
        }
    }

    /// <summary>
    /// Builds a normalized URI from a configured endpoint and a relative path.
    /// </summary>
    private static Uri BuildUri(string endpoint, string path)
    {
        var normalizedEndpoint = endpoint.TrimEnd('/');
        var normalizedPath = path.StartsWith('/')
            ? path
            : "/" + path;

        return new Uri(normalizedEndpoint + normalizedPath, UriKind.Absolute);
    }

    /// <summary>
    /// Formats an optional value for CLI output.
    /// </summary>
    private static string FormatOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }
}
