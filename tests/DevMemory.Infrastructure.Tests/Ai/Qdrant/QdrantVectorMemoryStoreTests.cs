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

            return CreateUnexpectedRequestResponse();
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
                Assert.Equal("feature/revisions", payload.GetProperty("branch").GetString());
                Assert.Equal("Revision cloning was handled by normalizing null collections.", payload.GetProperty("text").GetString());
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

            return CreateUnexpectedRequestResponse();
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

                using var json = JsonDocument.Parse(request.Body);
                var point = json.RootElement.GetProperty("points")[0];
                var payload = point.GetProperty("payload");

                Assert.Equal(memoryId.ToString("D"), point.GetProperty("id").GetString());
                Assert.Equal(memoryId.ToString("D"), payload.GetProperty("documentId").GetString());
                Assert.Equal("def456", payload.GetProperty("contentHash").GetString());
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

            return CreateUnexpectedRequestResponse();
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

    [Fact]
    public async Task UpsertAsync_WhenDocumentIdIsNotGuid_FallsBackToMemoryIdAsPointId()
    {
        // Arrange
        var memoryId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
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

            return CreateUnexpectedRequestResponse();
        }))
        {
            BaseAddress = new Uri("http://localhost:6333")
        };

        var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        var document = new VectorMemoryDocument
        {
            MemoryId = memoryId,
            DocumentId = "custom-non-guid-id",
            ContentHash = "hash789",
            Title = "Fallback identity",
            Text = "Fallback identity text.",
            Vector = [0.1f, 0.2f, 0.3f]
        };

        // Act
        await store.UpsertAsync(document, CancellationToken.None);

        // Assert
        var upsertRequest = Assert.Single(
            requests,
            request =>
                request.Method == HttpMethod.Put &&
                request.PathAndQuery == "/collections/devmemory_memories/points?wait=true");

        using var json = JsonDocument.Parse(upsertRequest.Body);
        var point = json.RootElement.GetProperty("points")[0];
        var payload = point.GetProperty("payload");

        Assert.Equal(memoryId.ToString("D"), point.GetProperty("id").GetString());
        Assert.Equal("custom-non-guid-id", payload.GetProperty("documentId").GetString());
        Assert.Equal("hash789", payload.GetProperty("contentHash").GetString());
    }

    [Fact]
    public async Task UpsertAsync_WhenCollectionCheckFails_ThrowsClearException()
    {
        // Arrange
        using var httpClient = new HttpClient(new FakeHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("""{"error":"qdrant unavailable"}""")
            })))
        {
            BaseAddress = new Uri("http://localhost:6333")
        };

        var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.UpsertAsync(
                BuildDocument(Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff")),
                CancellationToken.None));

        // Assert
        Assert.Contains("Qdrant collection check failed with status code 500.", exception.Message, StringComparison.Ordinal);
        Assert.Contains("qdrant unavailable", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpsertAsync_WhenExistingCollectionVectorSizeDoesNotMatch_ThrowsClearException()
    {
        // Arrange
        using var httpClient = new HttpClient(new FakeHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Get &&
                request.RequestUri?.PathAndQuery == "/collections/devmemory_memories")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                          "result": {
                            "config": {
                              "params": {
                                "vectors": {
                                  "size": 768,
                                  "distance": "Cosine"
                                }
                              }
                            }
                          }
                        }
                        """)
                });
            }

            return Task.FromResult(CreateUnexpectedRequestResponse());
        }))
        {
            BaseAddress = new Uri("http://localhost:6333")
        };

        var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        var document = BuildDocument(Guid.Parse("cccccccc-dddd-eeee-ffff-aaaaaaaaaaaa")) with
        {
            Vector = [0.1f, 0.2f, 0.3f]
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.UpsertAsync(document, CancellationToken.None));

        // Assert
        Assert.Contains(
            "Qdrant collection vector size mismatch.",
            exception.Message,
            StringComparison.Ordinal);

        Assert.Contains(
            "Existing size is 768, but current embedding size is 3.",
            exception.Message,
            StringComparison.Ordinal);

        Assert.Contains(
            "Use the same embedding model used to create the collection, or recreate/reindex the collection.",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task UpsertAsync_WhenExistingCollectionVectorSizeMatches_UpsertsPoint()
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
                    Content = new StringContent(
                        """
                        {
                          "result": {
                            "config": {
                              "params": {
                                "vectors": {
                                  "size": 3,
                                  "distance": "Cosine"
                                }
                              }
                            }
                          }
                        }
                        """)
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

            return CreateUnexpectedRequestResponse();
        }))
        {
            BaseAddress = new Uri("http://localhost:6333")
        };

        var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        var document = BuildDocument(Guid.Parse("dddddddd-eeee-ffff-aaaa-bbbbbbbbbbbb"));

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
    public async Task SearchAsync_WhenCollectionDoesNotExist_ReturnsEmptyResults()
    {
        // Arrange
        using var httpClient = new HttpClient(new FakeHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Post &&
                request.RequestUri?.PathAndQuery == "/collections/devmemory_memories/points/search")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("""{"status":"not_found"}""")
                });
            }

            return Task.FromResult(CreateUnexpectedRequestResponse());
        }))
        {
            BaseAddress = new Uri("http://localhost:6333")
        };

        var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        // Act
        var results = await store.SearchAsync(
            [0.1f, 0.2f, 0.3f],
            5,
            CancellationToken.None);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_WhenQdrantReturnsServerError_ThrowsClearException()
    {
        // Arrange
        using var httpClient = new HttpClient(new FakeHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Post &&
                request.RequestUri?.PathAndQuery == "/collections/devmemory_memories/points/search")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("""{"error":"qdrant search failed"}""")
                });
            }

            return Task.FromResult(CreateUnexpectedRequestResponse());
        }))
        {
            BaseAddress = new Uri("http://localhost:6333")
        };

        var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.SearchAsync(
                [0.1f, 0.2f, 0.3f],
                5,
                CancellationToken.None));

        // Assert
        Assert.Contains(
            "Qdrant search request failed with status code 500.",
            exception.Message,
            StringComparison.Ordinal);

        Assert.Contains(
            "qdrant search failed",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task TryGetIndexedContentHashAsync_WhenPointExists_ReturnsContentHash()
    {
        // Arrange
        var memoryId = Guid.Parse("741bf4b6-2b81-48a5-beae-1d0208e521d2");

        using var httpClient = new HttpClient(new FakeHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Post &&
                request.RequestUri?.PathAndQuery == "/collections/devmemory_memories/points")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                        "result": [
                            {
                            "id": "741bf4b6-2b81-48a5-beae-1d0208e521d2",
                            "payload": {
                                "contentHash": "abc123"
                            }
                            }
                        ]
                        }
                        """)
                });
            }

            return Task.FromResult(CreateUnexpectedRequestResponse());
        }))
        {
            BaseAddress = new Uri("http://localhost:6333")
        };

        var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        // Act
        var contentHash = await store.TryGetIndexedContentHashAsync(
            memoryId.ToString("D"),
            CancellationToken.None);

        // Assert
        Assert.Equal("abc123", contentHash);
    }

    [Fact]
    public async Task TryGetIndexedContentHashAsync_WhenCollectionDoesNotExist_ReturnsNull()
    {
        // Arrange
        using var httpClient = new HttpClient(new FakeHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Post &&
                request.RequestUri?.PathAndQuery == "/collections/devmemory_memories/points")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("""{"status":"not_found"}""")
                });
            }

            return Task.FromResult(CreateUnexpectedRequestResponse());
        }))
        {
            BaseAddress = new Uri("http://localhost:6333")
        };

        var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        // Act
        var contentHash = await store.TryGetIndexedContentHashAsync(
            "741bf4b6-2b81-48a5-beae-1d0208e521d2",
            CancellationToken.None);

        // Assert
        Assert.Null(contentHash);
    }

    [Fact]
    public async Task DeleteAsync_WhenCalled_SendsDeletePointRequest()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");

        using var httpClient = new HttpClient(new TestHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(
                "http://localhost:6333/collections/devmemory_memories/points/delete",
                request.RequestUri?.ToString());

            var requestContent = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

            Assert.NotNull(requestContent);
            Assert.Contains(memoryId.ToString("D"), requestContent, StringComparison.Ordinal);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"result":{"operation_id":1,"status":"completed"},"status":"ok","time":0.001}""")
            };
        }))
        {
            BaseAddress = new Uri("http://localhost:6333")
        };

        var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        // Act
        await store.DeleteAsync(memoryId, CancellationToken.None);
    }

    [Fact]
    public async Task DeleteAsync_WhenCollectionDoesNotExist_DoesNotThrow()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");

        using var httpClient = new HttpClient(new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("""{"status":{"error":"Not found"},"time":0.001}""")
            }))
        {
            BaseAddress = new Uri("http://localhost:6333")
        };

        var store = new QdrantVectorMemoryStore(httpClient, "devmemory_memories");

        // Act
        var exception = await Record.ExceptionAsync(
            () => store.DeleteAsync(memoryId, CancellationToken.None));

        // Assert
        Assert.Null(exception);
    }

    #region Helpers

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

    /// <summary>
    /// Creates a response for unexpected fake HTTP requests.
    /// </summary>
    private static HttpResponseMessage CreateUnexpectedRequestResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("""{"error":"unexpected request"}""")
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

    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }

    #endregion
}
