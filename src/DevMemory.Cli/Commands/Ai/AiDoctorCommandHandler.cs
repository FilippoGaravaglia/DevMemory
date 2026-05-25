using System.Net;
using System.Text.Json;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Cli.CommandLine;
using DevMemory.Infrastructure;

namespace DevMemory.Cli.Commands.Ai;

public sealed class AiDoctorCommandHandler : ICommandHandler
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(3);

    private static readonly JsonSerializerOptions OllamaJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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
    /// Checks Ollama connectivity and validates that configured models are available.
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
        var responseContent = TryGetHttpContent("Ollama", uri, out var isReachable);

        if (!isReachable)
        {
            return false;
        }

        return ValidateOllamaModels(options, responseContent);
    }

    /// <summary>
    /// Checks Qdrant connectivity and validates that the configured collection is available.
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

        var collectionsUri = BuildUri(options.VectorStore.QdrantEndpoint, "/collections");
        var _ = TryGetHttpContent("Qdrant", collectionsUri, out var isReachable);

        if (!isReachable)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(options.VectorStore.QdrantCollection))
        {
            Console.WriteLine("Collection status: missing configuration");
            Console.WriteLine("Reason: Qdrant collection name is missing.");

            return false;
        }

        var collectionUri = BuildUri(
            options.VectorStore.QdrantEndpoint,
            $"/collections/{Uri.EscapeDataString(options.VectorStore.QdrantCollection)}");

        return CheckQdrantCollection(collectionUri, options.VectorStore.QdrantCollection);
    }

    /// <summary>
    /// Performs a lightweight HTTP GET request and returns the response content when reachable.
    /// </summary>
    private string TryGetHttpContent(string name, Uri uri, out bool isReachable)
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

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Status: not healthy");

                isReachable = false;

                return string.Empty;
            }

            Console.WriteLine("Status: reachable");

            isReachable = true;

            return response.Content
                .ReadAsStringAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            Console.WriteLine($"Endpoint: {uri}");
            Console.WriteLine("Status: unreachable");
            Console.WriteLine($"Reason: {ex.Message}");

            isReachable = false;

            return string.Empty;
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

    /// <summary>
    /// Validates that the configured Ollama models are available locally.
    /// </summary>
    private static bool ValidateOllamaModels(
        AiRuntimeOptions options,
        string responseContent)
    {
        var availableModels = ParseOllamaModels(responseContent);

        var isValid = true;

        if (options.Chat.IsOllama)
        {
            isValid &= PrintOllamaModelStatus("Chat model", options.Chat.OllamaChatModel, availableModels);
        }

        if (options.Embedding.IsOllama)
        {
            isValid &= PrintOllamaModelStatus(
                "Embedding model",
                options.Embedding.OllamaEmbeddingModel,
                availableModels);
        }

        return isValid;
    }

    /// <summary>
    /// Parses the list of locally available Ollama models.
    /// </summary>
    private static string[] ParseOllamaModels(string responseContent)
    {
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return [];
        }

        try
        {
            var response = JsonSerializer.Deserialize<OllamaTagsResponse>(
                responseContent,
                OllamaJsonSerializerOptions);

            return response?.Models
                .Select(model => model.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToArray() ?? [];
        }
        catch (JsonException)
        {
            Console.WriteLine("Model status: unknown");
            Console.WriteLine("Reason: Ollama returned an unexpected response.");

            return [];
        }
    }

    /// <summary>
    /// Prints whether a configured Ollama model is available locally.
    /// </summary>
    private static bool PrintOllamaModelStatus(
        string label,
        string? modelName,
        IReadOnlyCollection<string> availableModels)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            Console.WriteLine($"{label} status: missing configuration");

            return false;
        }

        var exists = availableModels.Any(
            availableModel => availableModel.Equals(modelName, StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            Console.WriteLine($"{label} status: available ({modelName})");

            return true;
        }

        Console.WriteLine($"{label} status: missing ({modelName})");
        Console.WriteLine($"Hint: run ollama pull {modelName}");

        return false;
    }

    /// <summary>
    /// Checks whether the configured Qdrant collection exists.
    /// </summary>
    private bool CheckQdrantCollection(Uri collectionUri, string collectionName)
    {
        using var httpClient = _httpClientFactory();
        httpClient.Timeout = DefaultTimeout;

        try
        {
            using var response = httpClient
                .GetAsync(collectionUri, CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Collection status: missing ({collectionName})");
                Console.WriteLine("Hint: run devmemory index to create and populate the collection.");

                return false;
            }

            Console.WriteLine($"Collection endpoint: {collectionUri}");
            Console.WriteLine($"Collection status code: {(int)response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Collection status: not healthy");

                return false;
            }

            Console.WriteLine($"Collection status: available ({collectionName})");

            return true;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            Console.WriteLine($"Collection endpoint: {collectionUri}");
            Console.WriteLine("Collection status: unreachable");
            Console.WriteLine($"Reason: {ex.Message}");

            return false;
        }
    }

    /// <summary>
    /// Represents the minimal Ollama tags response used by the doctor command.
    /// </summary>
    private sealed record OllamaTagsResponse
    {
        public IReadOnlyCollection<OllamaModelInfo> Models { get; init; } = [];
    }

    /// <summary>
    /// Represents the minimal Ollama model info used by the doctor command.
    /// </summary>
    private sealed record OllamaModelInfo
    {
        public string Name { get; init; } = string.Empty;
    }
}
