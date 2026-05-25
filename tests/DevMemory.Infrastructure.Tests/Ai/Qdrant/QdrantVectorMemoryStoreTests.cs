using System.Net;
using System.Text.Json;
using DevMemory.Application.Models.Ai.VectorStore;
using DevMemory.Infrastructure.Ai.Qdrant;

namespace DevMemory.Infrastructure.Tests.Ai.Qdrant;

public sealed class QdrantVectorMemoryStoreTests
{
    [Fact]
    public async Task UpsertAsync_WhenCollectionDoesNotExist_CreatesCollectionAndUpsertsPoint()
    {
        // Arrange
        var memoryId = Guid.Parse("741bf4b6-2b81-48a5-beae-1d0208e521d2");
        var requests = new List<CapturedRequest>();

        using var httpClient = new HttpClient(new FakeHttpMessageHandler(async request =>
        {
            requests.Add(await CapturedRequest.FromAsync(request));

            if (request.Method == HttpMethod.Get &&
                request.RequestUri?.PathAndQuery == "/collections/devmemory_memories")
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("""{"status":"not_found"}""")
                };
            }

            if (request.Method == HttpMethod.Put &&
                request.RequestUri?.PathAndQuery == "/collections/devmemory_memories")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"result":true}""")
                };
            }

            if (request.Method == HttpMethod.Put &&
                request.RequestUri?.PathAndQuery == "/collections/devmemory_memories/points?wait=true")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"result":{"operation_id":1,"status":"completed"}}""")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("""{"error":"unexpected request"}""")
            };
        }))
        {
            BaseAddress = new Uri("http://localhost:6333")
        };

        var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        var document = new VectorMemoryDocument
        {
            MemoryId = memoryId,
            DocumentId = memoryId.ToString("D"),
            ContentHash = "abc123",
            Title = "Estimate revision cloning",
            Project = "LogicalCommon",
            Area = "Estimate",
            Branch = "feature/revisions",
            Tags = ["dotnet", "mongodb"],
            FilesTouched = ["src/EstimateService.cs"],
            Text = "Revision cloning was handled by normalizing null collections.",
            Vector = [0.1f, 0.2f, 0.3f]
        };

        // Act
        await store.UpsertAsync(document, CancellationToken.None);

        // Assert
        Assert.Collection(
            requests,
            request =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.Equal("/collections/devmemory_memories", request.PathAndQuery);
            },
            request =>
            {
                Assert.Equal(HttpMethod.Put, request.Method);
                Assert.Equal("/collections/devmemory_memories", request.PathAndQuery);

                using var json = JsonDocument.Parse(request.Body);
                var vectors = json.RootElement.GetProperty("vectors");

                Assert.Equal(3, vectors.GetProperty("size").GetInt32());
                Assert.Equal("Cosine", vectors.GetProperty("distance").GetString());
            },
            request =>
            {
                Assert.Equal(HttpMethod.Put, request.Method);
                Assert.Equal("/collections/devmemory_memories/points?wait=true", request.PathAndQuery);

                using var json = JsonDocument.Parse(request.Body);
                var point = json.RootElement.GetProperty("points")[0];

                Assert.Equal(memoryId.ToString("D"), point.GetProperty("id").GetString());

                var payload = point.GetProperty("payload");

                Assert.Equal(memoryId.ToString("D"), payload.GetProperty("memoryId").GetString());
                Assert.Equal(memoryId.ToString("D"), payload.GetProperty("documentId").GetString());
                Assert.Equal("abc123", payload.GetProperty("contentHash").GetString());
                Assert.Equal("Estimate revision cloning", payload.GetProperty("title").GetString());
                Assert.Equal("LogicalCommon", payload.GetProperty("project").GetString());
                Assert.Equal("Estimate", payload.GetProperty("area").GetString());
            });
    }

    [Fact]
    public async Task UpsertAsync_WhenCollectionAlreadyExists_DoesNotCreateCollection()
    {
        // Arrange
        var memoryId = Guid.Parse("39478526-4706-455e-9444-e18d01771240");
        var requests = new List<CapturedRequest>();

        using var httpClient = new HttpClient(new FakeHttpMessageHandler(async request =>
        {
            requests.Add(await CapturedRequest.FromAsync(request));

            if (request.Method == HttpMethod.Get &&
                request.RequestUri?.PathAndQuery == "/collections/devmemory_memories")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"result":{"status":"green"}}""")
                };
            }

            if (request.Method == HttpMethod.Put &&
                request.RequestUri?.PathAndQuery == "/collections/devmemory_memories/points?wait=true")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"result":{"operation_id":1,"status":"completed"}}""")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("""{"error":"unexpected request"}""")
            };
        }))
        {
            BaseAddress = new Uri("http://localhost:6333")
        };

        var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        var document = new VectorMemoryDocument
        {
            MemoryId = memoryId,
            DocumentId = memoryId.ToString("D"),
            ContentHash = "def456",
            Title = "MongoDB mapping fix",
            Text = "Fixed the Mapster mapping configuration.",
            Vector = [0.1f, 0.2f, 0.3f]
        };

        // Act
        await store.UpsertAsync(document, CancellationToken.None);

        // Assert
        Assert.Collection(
            requests,
            request =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.Equal("/collections/devmemory_memories", request.PathAndQuery);
            },
            request =>
            {
                Assert.Equal(HttpMethod.Put, request.Method);
                Assert.Equal("/collections/devmemory_memories/points?wait=true", request.PathAndQuery);
            });
    }

    [Fact]
    public async Task UpsertAsync_WhenCalledTwiceWithSameVectorSize_ChecksCollectionOnlyOnce()
    {
        // Arrange
        var requests = new List<CapturedRequest>();

        using var httpClient = new HttpClient(new FakeHttpMessageHandler(async request =>
        {
            requests.Add(await CapturedRequest.FromAsync(request));

            if (request.Method == HttpMethod.Get &&
                request.RequestUri?.PathAndQuery == "/collections/devmemory_memories")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"result":{"status":"green"}}""")
                };
            }

            if (request.Method == HttpMethod.Put &&
                request.RequestUri?.PathAndQuery == "/collections/devmemory_memories/points?wait=true")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"result":{"operation_id":1,"status":"completed"}}""")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("""{"error":"unexpected request"}""")
            };
        }))
        {
            BaseAddress = new Uri("http://localhost:6333")
        };

        var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        var firstDocument = BuildDocument(Guid.Parse("741bf4b6-2b81-48a5-beae-1d0208e521d2"));
        var secondDocument = BuildDocument(Guid.Parse("39478526-4706-455e-9444-e18d01771240"));

        // Act
        await store.UpsertAsync(firstDocument, CancellationToken.None);
        await store.UpsertAsync(secondDocument, CancellationToken.None);

        // Assert
        Assert.Equal(
            1,
            requests.Count(request =>
                request.Method == HttpMethod.Get &&
                request.PathAndQuery == "/collections/devmemory_memories"));

        Assert.Equal(
            2,
            requests.Count(request =>
                request.Method == HttpMethod.Put &&
                request.PathAndQuery == "/collections/devmemory_memories/points?wait=true"));
    }

    /// <summary>
    /// Builds a valid vector memory document for Qdrant tests.
    /// </summary>
    private static VectorMemoryDocument BuildDocument(Guid memoryId)
    {
        return new VectorMemoryDocument
        {
            MemoryId = memoryId,
            DocumentId = memoryId.ToString("D"),
            ContentHash = "abc123",
            Title = "Test memory",
            Text = "Test memory text.",
            Vector = [0.1f, 0.2f, 0.3f]
        };
    }

    private sealed record CapturedRequest(
        HttpMethod Method,
        string PathAndQuery,
        string Body)
    {
        /// <summary>
        /// Creates a captured request from an HTTP request message.
        /// </summary>
        public static async Task<CapturedRequest> FromAsync(HttpRequestMessage request)
        {
            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync();

            return new CapturedRequest(
                request.Method,
                request.RequestUri?.PathAndQuery ?? string.Empty,
                body);
        }
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _responseFactory;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFactory)
        {
            _responseFactory = responseFactory ?? throw new ArgumentNullException(nameof(responseFactory));
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return _responseFactory(request);
        }
    }
}
