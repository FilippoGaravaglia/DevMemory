using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevMemory.Application.Abstractions.Ai;
using DevMemory.Application.Models.Ai;

namespace DevMemory.Infrastructure.Ai.Ollama;

public sealed class OllamaEmbeddingService : IEmbeddingService, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = JsonSerializerOptions.Web;

    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    public OllamaEmbeddingService(string endpoint)
        : this(CreateHttpClient(endpoint), disposeHttpClient: true)
    {
    }

    public OllamaEmbeddingService(HttpClient httpClient)
        : this(httpClient, disposeHttpClient: false)
    {
    }

    private OllamaEmbeddingService(HttpClient httpClient, bool disposeHttpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
    }

    public async Task<EmbeddingResponse> GenerateEmbeddingAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

        var ollamaRequest = new OllamaEmbedRequest
        {
            Model = request.Model,
            Input = request.Text
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "/api/embed",
            ollamaRequest,
            JsonOptions,
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Ollama embedding request failed with status code {(int)response.StatusCode}. Response: {responseContent}");
        }

        var ollamaResponse = JsonSerializer.Deserialize<OllamaEmbedResponse>(
            responseContent,
            JsonOptions);

        var vector = ExtractSingleEmbeddingVector(ollamaResponse);

        return new EmbeddingResponse
        {
            Vector = vector,
            Provider = AiProviderNames.Ollama,
            Model = string.IsNullOrWhiteSpace(ollamaResponse?.Model)
                ? request.Model
                : ollamaResponse.Model
        };
    }

    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    /// <summary>
    /// Creates the HTTP client used to communicate with Ollama.
    /// </summary>
    private static HttpClient CreateHttpClient(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("Ollama endpoint cannot be empty.", nameof(endpoint));
        }

        return new HttpClient
        {
            BaseAddress = new Uri(endpoint, UriKind.Absolute)
        };
    }

    /// <summary>
    /// Validates the provider-independent embedding request before sending it to Ollama.
    /// </summary>
    private static void ValidateRequest(EmbeddingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Model))
        {
            throw new ArgumentException("Embedding model cannot be empty.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            throw new ArgumentException("Embedding text cannot be empty.", nameof(request));
        }
    }

    /// <summary>
    /// Extracts a single vector from the Ollama embedding response.
    /// </summary>
    private static IReadOnlyList<float> ExtractSingleEmbeddingVector(OllamaEmbedResponse? response)
    {
        var vector = response?.Embeddings.FirstOrDefault();

        if (vector is null || vector.Count == 0)
        {
            throw new InvalidOperationException("Ollama embedding response did not contain any vector.");
        }

        return vector;
    }

    private sealed record OllamaEmbedRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; init; } = string.Empty;

        [JsonPropertyName("input")]
        public string Input { get; init; } = string.Empty;
    }

    private sealed record OllamaEmbedResponse
    {
        [JsonPropertyName("model")]
        public string Model { get; init; } = string.Empty;

        [JsonPropertyName("embeddings")]
        public IReadOnlyCollection<IReadOnlyList<float>> Embeddings { get; init; } = [];
    }
}
