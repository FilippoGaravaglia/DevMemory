using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevMemory.Application.Abstractions;
using DevMemory.Application.Models.Ai.Chat;
using DevMemory.Application.Models.Ai.Runtime;

namespace DevMemory.Infrastructure.Ai.Ollama;

public sealed class OllamaChatCompletionService : IChatCompletionService, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = JsonSerializerOptions.Web;

    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    public OllamaChatCompletionService(string endpoint)
        : this(CreateHttpClient(endpoint), disposeHttpClient: true)
    {
    }

    public OllamaChatCompletionService(HttpClient httpClient)
        : this(httpClient, disposeHttpClient: false)
    {
    }

    private OllamaChatCompletionService(HttpClient httpClient, bool disposeHttpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
    }

    public async Task<ChatCompletionResponse> CompleteAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateRequest(request);

        var ollamaRequest = new OllamaChatRequest
        {
            Model = request.Model,
            Messages = request.Messages
                .Select(message => new OllamaChatMessage
                {
                    Role = message.Role,
                    Content = message.Content
                })
                .ToArray(),
            Stream = false,
            Options = new OllamaChatOptions
            {
                Temperature = Convert.ToDouble(request.Temperature)
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "/api/chat",
            ollamaRequest,
            JsonOptions,
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Ollama chat request failed with status code {(int)response.StatusCode}. Response: {responseContent}");
        }

        var ollamaResponse = JsonSerializer.Deserialize<OllamaChatResponse>(
            responseContent,
            JsonOptions);

        if (string.IsNullOrWhiteSpace(ollamaResponse?.Message?.Content))
        {
            throw new InvalidOperationException("Ollama chat response did not contain any assistant content.");
        }

        return new ChatCompletionResponse
        {
            Content = ollamaResponse.Message.Content,
            Provider = AiProviderNames.Ollama,
            Model = string.IsNullOrWhiteSpace(ollamaResponse.Model)
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
    /// Validates the provider-independent chat completion request before sending it to Ollama.
    /// </summary>
    private static void ValidateRequest(ChatCompletionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Model))
        {
            throw new ArgumentException("Chat model cannot be empty.", nameof(request));
        }

        if (request.Messages.Count == 0)
        {
            throw new ArgumentException("At least one chat message is required.", nameof(request));
        }

        if (request.Messages.Any(message => string.IsNullOrWhiteSpace(message.Content)))
        {
            throw new ArgumentException("Chat messages cannot contain empty content.", nameof(request));
        }
    }

    private sealed record OllamaChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; init; } = string.Empty;

        [JsonPropertyName("messages")]
        public IReadOnlyCollection<OllamaChatMessage> Messages { get; init; } = [];

        [JsonPropertyName("stream")]
        public bool Stream { get; init; }

        [JsonPropertyName("options")]
        public OllamaChatOptions Options { get; init; } = new();
    }

    private sealed record OllamaChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; init; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; init; } = string.Empty;
    }

    private sealed record OllamaChatOptions
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; init; }
    }

    private sealed record OllamaChatResponse
    {
        [JsonPropertyName("model")]
        public string Model { get; init; } = string.Empty;

        [JsonPropertyName("message")]
        public OllamaChatMessage? Message { get; init; }
    }
}
