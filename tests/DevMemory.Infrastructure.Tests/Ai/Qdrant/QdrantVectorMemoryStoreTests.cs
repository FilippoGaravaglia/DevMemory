using System.Net;
using System.Text;
using DevMemory.Application.Models.Ai;
using DevMemory.Infrastructure.Ai.Qdrant;

namespace DevMemory.Infrastructure.Tests;

public sealed class QdrantVectorMemoryStoreTests
{
    [Fact]
    public async Task UpsertAsync_WhenRequestIsSuccessful_CompletesSuccessfully()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, """{"result": "ok"}""");
        using var httpClient = CreateHttpClient(handler);
        using var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        var memoryId = Guid.NewGuid();

        var document = new VectorMemoryDocument
        {
            MemoryId = memoryId,
            Title = "Estimate revision cloning",
            Project = "DevMemory",
            Area = "AI",
            Branch = "main",
            Tags = ["dotnet", "rag"],
            FilesTouched = ["src/DevMemory.Infrastructure/Ai/Qdrant/QdrantVectorMemoryStore.cs"],
            Text = "Implemented estimate revision cloning.",
            Vector = [0.1f, 0.2f, 0.3f]
        };

        // Act
        await store.UpsertAsync(document, CancellationToken.None);

        // Assert
        Assert.Equal(HttpMethod.Put, handler.LastRequestMethod);
        Assert.Equal(
            "/collections/devmemory_memories/points?wait=true",
            handler.LastRequestPathAndQuery);
        Assert.Contains($"\"id\":\"{memoryId:D}\"", handler.LastRequestBody, StringComparison.Ordinal);
        Assert.Contains($"\"memoryId\":\"{memoryId:D}\"", handler.LastRequestBody, StringComparison.Ordinal);
        Assert.Contains("\"title\":\"Estimate revision cloning\"", handler.LastRequestBody, StringComparison.Ordinal);
        Assert.Contains("\"project\":\"DevMemory\"", handler.LastRequestBody, StringComparison.Ordinal);
        Assert.Contains("\"area\":\"AI\"", handler.LastRequestBody, StringComparison.Ordinal);
        Assert.Contains("\"branch\":\"main\"", handler.LastRequestBody, StringComparison.Ordinal);
        Assert.Contains("\"text\":\"Implemented estimate revision cloning.\"", handler.LastRequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpsertAsync_WhenRequestFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError, "server error");
        using var httpClient = CreateHttpClient(handler);
        using var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        var document = new VectorMemoryDocument
        {
            MemoryId = Guid.NewGuid(),
            Title = "Test",
            Text = "test",
            Vector = [0.1f]
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.UpsertAsync(document, CancellationToken.None));

        // Assert
        Assert.Contains("Qdrant upsert request failed", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SearchAsync_WhenRequestIsSuccessful_ReturnsMappedResults()
    {
        // Arrange
        var memoryId = Guid.NewGuid();

        var responseContent = $$"""
        {
          "result": [
            {
              "id": "{{memoryId:D}}",
              "score": 0.91,
              "payload": {
                "memoryId": "{{memoryId:D}}",
                "title": "Estimate revision cloning",
                "project": "DevMemory",
                "area": "AI",
                "branch": "main",
                "text": "Implemented estimate revision cloning."
              }
            }
          ]
        }
        """;

        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseContent);
        using var httpClient = CreateHttpClient(handler);
        using var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        // Act
        var results = await store.SearchAsync([0.1f, 0.2f, 0.3f], 5, CancellationToken.None);

        // Assert
        var result = Assert.Single(results);

        Assert.Equal(memoryId, result.MemoryId);
        Assert.Equal("Estimate revision cloning", result.Title);
        Assert.Equal("DevMemory", result.Project);
        Assert.Equal("AI", result.Area);
        Assert.Equal("Implemented estimate revision cloning.", result.Text);
        Assert.Equal(0.91m, result.Score);
        Assert.Equal(HttpMethod.Post, handler.LastRequestMethod);
        Assert.Equal(
            "/collections/devmemory_memories/points/search",
            handler.LastRequestPathAndQuery);
        Assert.Contains("\"limit\":5", handler.LastRequestBody, StringComparison.Ordinal);
        Assert.Contains("\"with_payload\":true", handler.LastRequestBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SearchAsync_WhenRequestFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError, "server error");
        using var httpClient = CreateHttpClient(handler);
        using var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.SearchAsync([0.1f], 5, CancellationToken.None));

        // Assert
        Assert.Contains("Qdrant search request failed", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SearchAsync_WhenVectorIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{}");
        using var httpClient = CreateHttpClient(handler);
        using var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => store.SearchAsync([], 5, CancellationToken.None));

        // Assert
        Assert.Contains("Search vector cannot be empty.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SearchAsync_WhenLimitIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{}");
        using var httpClient = CreateHttpClient(handler);
        using var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => store.SearchAsync([0.1f], 0, CancellationToken.None));

        // Assert
        Assert.Contains("Search limit must be greater than zero.", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates an HTTP client backed by a fake response handler.
    /// </summary>
    private static HttpClient CreateHttpClient(FakeHttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:6333", UriKind.Absolute)
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

        public HttpMethod? LastRequestMethod { get; private set; }

        public string LastRequestPathAndQuery { get; private set; } = string.Empty;

        public string LastRequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequestMethod = request.Method;
            LastRequestPathAndQuery = request.RequestUri?.PathAndQuery ?? string.Empty;
            LastRequestBody = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent, Encoding.UTF8, "application/json")
            };
        }
    }
}
