using System.Net;
using System.Text;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Infrastructure.Ai.Ollama;

namespace DevMemory.Infrastructure.Tests.Ai.Ollama;

public sealed class OllamaEmbeddingServiceTests
{
    [Fact]
    public async Task GenerateEmbeddingAsync_WhenResponseIsSuccessful_ReturnsEmbeddingResponse()
    {
        // Arrange
        const string responseContent = """
        {
          "model": "nomic-embed-text",
          "embeddings": [
            [0.1, 0.2, 0.3]
          ]
        }
        """;

        using var httpClient = CreateHttpClient(HttpStatusCode.OK, responseContent);
        using var service = new OllamaEmbeddingService(httpClient);

        var request = new EmbeddingRequest
        {
            Model = "nomic-embed-text",
            Text = "How to clone an estimate revision"
        };

        // Act
        var response = await service.GenerateEmbeddingAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(AiProviderNames.Ollama, response.Provider);
        Assert.Equal("nomic-embed-text", response.Model);
        Assert.Equal([0.1f, 0.2f, 0.3f], response.Vector);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WhenRequestFails_ThrowsInvalidOperationException()
    {
        // Arrange
        using var httpClient = CreateHttpClient(HttpStatusCode.InternalServerError, "server error");
        using var service = new OllamaEmbeddingService(httpClient);

        var request = new EmbeddingRequest
        {
            Model = "nomic-embed-text",
            Text = "test"
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GenerateEmbeddingAsync(request, CancellationToken.None));

        // Assert
        Assert.Contains("Ollama embedding request failed", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WhenVectorIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        const string responseContent = """
        {
          "model": "nomic-embed-text",
          "embeddings": []
        }
        """;

        using var httpClient = CreateHttpClient(HttpStatusCode.OK, responseContent);
        using var service = new OllamaEmbeddingService(httpClient);

        var request = new EmbeddingRequest
        {
            Model = "nomic-embed-text",
            Text = "test"
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GenerateEmbeddingAsync(request, CancellationToken.None));

        // Assert
        Assert.Contains("Ollama embedding response did not contain any vector.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WhenModelIsMissing_ThrowsArgumentException()
    {
        // Arrange
        using var httpClient = CreateHttpClient(HttpStatusCode.OK, "{}");
        using var service = new OllamaEmbeddingService(httpClient);

        var request = new EmbeddingRequest
        {
            Model = string.Empty,
            Text = "test"
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.GenerateEmbeddingAsync(request, CancellationToken.None));

        // Assert
        Assert.Contains("Embedding model cannot be empty.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_WhenTextIsMissing_ThrowsArgumentException()
    {
        // Arrange
        using var httpClient = CreateHttpClient(HttpStatusCode.OK, "{}");
        using var service = new OllamaEmbeddingService(httpClient);

        var request = new EmbeddingRequest
        {
            Model = "nomic-embed-text",
            Text = string.Empty
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.GenerateEmbeddingAsync(request, CancellationToken.None));

        // Assert
        Assert.Contains("Embedding text cannot be empty.", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates an HTTP client backed by a fake response handler.
    /// </summary>
    private static HttpClient CreateHttpClient(HttpStatusCode statusCode, string responseContent)
    {
        return new HttpClient(new FakeHttpMessageHandler(statusCode, responseContent))
        {
            BaseAddress = new Uri("http://localhost:11434", UriKind.Absolute)
        };
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseContent;

        public FakeHttpMessageHandler(HttpStatusCode statusCode, string responseContent)
        {
            _statusCode = statusCode;
            _responseContent = responseContent;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
