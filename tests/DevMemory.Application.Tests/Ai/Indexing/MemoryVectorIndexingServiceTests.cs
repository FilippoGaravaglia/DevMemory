using DevMemory.Application.Abstractions;
using DevMemory.Application.Ai.Indexing;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;

namespace DevMemory.Application.Tests.Ai.Indexing;

public sealed class MemoryVectorIndexingServiceTests
{
    [Fact]
    public async Task IndexAsync_WhenDocumentsAreValid_GeneratesEmbeddingsAndUpsertsDocuments()
    {
        // Arrange
        var embeddingService = new FakeEmbeddingService();
        var vectorStore = new FakeVectorMemoryStore();

        var service = new MemoryVectorIndexingService(embeddingService, vectorStore);

        var memoryId = Guid.NewGuid();

        var documents = new[]
        {
            new VectorMemoryDocument
            {
                MemoryId = memoryId,
                DocumentId = memoryId.ToString("D"),
                ContentHash = "hash-1",
                Title = "Estimate revision cloning",
                Project = "DevMemory",
                Area = "AI",
                Branch = "main",
                Tags = ["dotnet", "rag"],
                FilesTouched = ["src/file.cs"],
                Text = "Implemented estimate revision cloning."
            }
        };

        // Act
        var result = await service.IndexAsync(
            documents,
            "nomic-embed-text",
            CancellationToken.None);

        // Assert
        Assert.Equal(1, result.TotalDocuments);
        Assert.Equal(1, result.IndexedDocuments);
        Assert.Equal(0, result.SkippedDocuments);
        Assert.Equal(0, result.FailedDocuments);
        Assert.Empty(result.Errors);

        var indexedDocument = Assert.Single(vectorStore.Documents);

        Assert.Equal(memoryId, indexedDocument.MemoryId);
        Assert.Equal(memoryId.ToString("D"), indexedDocument.DocumentId);
        Assert.Equal("hash-1", indexedDocument.ContentHash);
        Assert.Equal("Estimate revision cloning", indexedDocument.Title);
        Assert.Equal("DevMemory", indexedDocument.Project);
        Assert.Equal("AI", indexedDocument.Area);
        Assert.Equal([0.1f, 0.2f, 0.3f], indexedDocument.Vector);

        var embeddingRequest = Assert.Single(embeddingService.Requests);

        Assert.Equal("nomic-embed-text", embeddingRequest.Model);
        Assert.Equal("Implemented estimate revision cloning.", embeddingRequest.Text);
    }

    [Fact]
    public async Task IndexAsync_WhenDocumentHashIsAlreadyIndexed_SkipsEmbeddingAndUpsert()
    {
        // Arrange
        var memoryId = Guid.Parse("741bf4b6-2b81-48a5-beae-1d0208e521d2");

        var document = new VectorMemoryDocument
        {
            MemoryId = memoryId,
            DocumentId = memoryId.ToString("D"),
            ContentHash = "hash-1",
            Title = "Already indexed memory",
            Text = "This memory was already indexed."
        };

        var embeddingService = new FakeEmbeddingService();
        var vectorStore = new FakeIncrementalVectorMemoryStore(indexedContentHash: "hash-1");

        var service = new MemoryVectorIndexingService(embeddingService, vectorStore);

        // Act
        var result = await service.IndexAsync(
            [document],
            "nomic-embed-text",
            CancellationToken.None);

        // Assert
        Assert.Equal(1, result.TotalDocuments);
        Assert.Equal(0, result.IndexedDocuments);
        Assert.Equal(1, result.SkippedDocuments);
        Assert.Equal(0, result.FailedDocuments);
        Assert.Empty(result.Errors);

        Assert.Empty(embeddingService.Requests);
        Assert.Empty(vectorStore.Documents);
        Assert.Equal(1, vectorStore.ReadStateCalls);
    }

    [Fact]
    public async Task IndexAsync_WhenDocumentHashChanged_GeneratesEmbeddingAndUpsertsDocument()
    {
        // Arrange
        var memoryId = Guid.Parse("39478526-4706-455e-9444-e18d01771240");

        var document = new VectorMemoryDocument
        {
            MemoryId = memoryId,
            DocumentId = memoryId.ToString("D"),
            ContentHash = "hash-new",
            Title = "Updated memory",
            Text = "This memory changed."
        };

        var embeddingService = new FakeEmbeddingService();
        var vectorStore = new FakeIncrementalVectorMemoryStore(indexedContentHash: "hash-old");

        var service = new MemoryVectorIndexingService(embeddingService, vectorStore);

        // Act
        var result = await service.IndexAsync(
            [document],
            "nomic-embed-text",
            CancellationToken.None);

        // Assert
        Assert.Equal(1, result.TotalDocuments);
        Assert.Equal(1, result.IndexedDocuments);
        Assert.Equal(0, result.SkippedDocuments);
        Assert.Equal(0, result.FailedDocuments);
        Assert.Empty(result.Errors);

        Assert.Single(embeddingService.Requests);
        Assert.Single(vectorStore.Documents);
        Assert.Equal(1, vectorStore.ReadStateCalls);
    }

    [Fact]
    public async Task IndexAsync_WhenVectorStoreDoesNotSupportStateLookup_IndexesDocument()
    {
        // Arrange
        var memoryId = Guid.Parse("c4481c81-4d7e-4033-abb5-a16e30748bf3");

        var document = new VectorMemoryDocument
        {
            MemoryId = memoryId,
            DocumentId = memoryId.ToString("D"),
            ContentHash = "hash-1",
            Title = "Regular store memory",
            Text = "This memory should be indexed because the store has no state lookup."
        };

        var embeddingService = new FakeEmbeddingService();
        var vectorStore = new FakeVectorMemoryStore();

        var service = new MemoryVectorIndexingService(embeddingService, vectorStore);

        // Act
        var result = await service.IndexAsync(
            [document],
            "nomic-embed-text",
            CancellationToken.None);

        // Assert
        Assert.Equal(1, result.TotalDocuments);
        Assert.Equal(1, result.IndexedDocuments);
        Assert.Equal(0, result.SkippedDocuments);
        Assert.Equal(0, result.FailedDocuments);
        Assert.Empty(result.Errors);

        Assert.Single(embeddingService.Requests);
        Assert.Single(vectorStore.Documents);
    }

    [Fact]
    public async Task IndexAsync_WhenDocumentDoesNotHaveContentHash_IndexesDocument()
    {
        // Arrange
        var memoryId = Guid.Parse("dddddddd-eeee-ffff-aaaa-bbbbbbbbbbbb");

        var document = new VectorMemoryDocument
        {
            MemoryId = memoryId,
            DocumentId = memoryId.ToString("D"),
            ContentHash = string.Empty,
            Title = "Missing hash memory",
            Text = "This memory should still be indexed."
        };

        var embeddingService = new FakeEmbeddingService();
        var vectorStore = new FakeIncrementalVectorMemoryStore(indexedContentHash: "hash-1");

        var service = new MemoryVectorIndexingService(embeddingService, vectorStore);

        // Act
        var result = await service.IndexAsync(
            [document],
            "nomic-embed-text",
            CancellationToken.None);

        // Assert
        Assert.Equal(1, result.TotalDocuments);
        Assert.Equal(1, result.IndexedDocuments);
        Assert.Equal(0, result.SkippedDocuments);
        Assert.Equal(0, result.FailedDocuments);
        Assert.Empty(result.Errors);

        Assert.Single(embeddingService.Requests);
        Assert.Single(vectorStore.Documents);
        Assert.Equal(0, vectorStore.ReadStateCalls);
    }

    [Fact]
    public async Task IndexAsync_WhenEmbeddingModelIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var service = new MemoryVectorIndexingService(
            new FakeEmbeddingService(),
            new FakeVectorMemoryStore());

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.IndexAsync([], string.Empty, CancellationToken.None));

        // Assert
        Assert.Contains("Embedding model cannot be empty.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task IndexAsync_WhenDocumentIsInvalid_ReturnsFailureResult()
    {
        // Arrange
        var service = new MemoryVectorIndexingService(
            new FakeEmbeddingService(),
            new FakeVectorMemoryStore());

        var documents = new[]
        {
            new VectorMemoryDocument
            {
                MemoryId = Guid.Empty,
                Text = "Invalid document."
            }
        };

        // Act
        var result = await service.IndexAsync(
            documents,
            "nomic-embed-text",
            CancellationToken.None);

        // Assert
        Assert.Equal(1, result.TotalDocuments);
        Assert.Equal(0, result.IndexedDocuments);
        Assert.Equal(0, result.SkippedDocuments);
        Assert.Equal(1, result.FailedDocuments);

        var error = Assert.Single(result.Errors);

        Assert.Contains("MemoryId=<empty>", error, StringComparison.Ordinal);
        Assert.Contains("Vector document memory id cannot be empty.", error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task IndexAsync_WhenOneDocumentFails_ContinuesIndexingRemainingDocuments()
    {
        // Arrange
        var embeddingService = new FakeEmbeddingService
        {
            ShouldFailForText = "This one fails."
        };

        var vectorStore = new FakeVectorMemoryStore();

        var service = new MemoryVectorIndexingService(embeddingService, vectorStore);

        var successfulMemoryId = Guid.NewGuid();
        var failedMemoryId = Guid.NewGuid();

        var documents = new[]
        {
            new VectorMemoryDocument
            {
                MemoryId = failedMemoryId,
                DocumentId = failedMemoryId.ToString("D"),
                ContentHash = "hash-failed",
                Text = "This one fails."
            },
            new VectorMemoryDocument
            {
                MemoryId = successfulMemoryId,
                DocumentId = successfulMemoryId.ToString("D"),
                ContentHash = "hash-success",
                Text = "This one succeeds."
            }
        };

        // Act
        var result = await service.IndexAsync(
            documents,
            "nomic-embed-text",
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalDocuments);
        Assert.Equal(1, result.IndexedDocuments);
        Assert.Equal(0, result.SkippedDocuments);
        Assert.Equal(1, result.FailedDocuments);
        Assert.Single(result.Errors);

        var indexedDocument = Assert.Single(vectorStore.Documents);

        Assert.Equal(successfulMemoryId, indexedDocument.MemoryId);
        Assert.Equal([0.1f, 0.2f, 0.3f], indexedDocument.Vector);
    }

    [Fact]
    public async Task IndexAsync_WhenCancellationIsRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var service = new MemoryVectorIndexingService(
            new FakeEmbeddingService(),
            new FakeVectorMemoryStore());

        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        var documents = new[]
        {
            new VectorMemoryDocument
            {
                MemoryId = Guid.NewGuid(),
                Text = "Cancelled document."
            }
        };

        // Act / Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.IndexAsync(
                documents,
                "nomic-embed-text",
                cancellationTokenSource.Token));
    }

    [Fact]
    public async Task IndexAsync_WhenForceIsTrue_IgnoresAlreadyIndexedHashAndReindexesDocument()
    {
        // Arrange
        var memoryId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        var document = new VectorMemoryDocument
        {
            MemoryId = memoryId,
            DocumentId = memoryId.ToString("D"),
            ContentHash = "hash-1",
            Title = "Force indexed memory",
            Text = "This memory should be indexed even if the hash is already present."
        };

        var embeddingService = new FakeEmbeddingService();
        var vectorStore = new FakeIncrementalVectorMemoryStore(indexedContentHash: "hash-1");

        var service = new MemoryVectorIndexingService(embeddingService, vectorStore);

        // Act
        var result = await service.IndexAsync(
            [document],
            "nomic-embed-text",
            force: true,
            CancellationToken.None);

        // Assert
        Assert.Equal(1, result.TotalDocuments);
        Assert.Equal(1, result.IndexedDocuments);
        Assert.Equal(0, result.SkippedDocuments);
        Assert.Equal(0, result.FailedDocuments);
        Assert.Empty(result.Errors);

        Assert.Single(embeddingService.Requests);
        Assert.Single(vectorStore.Documents);

        // Force mode must not need a state lookup before indexing.
        Assert.Equal(0, vectorStore.ReadStateCalls);
    }

    #region Helpers

    private sealed class FakeEmbeddingService : IEmbeddingService
    {
        public List<EmbeddingRequest> Requests { get; } = [];

        public string? ShouldFailForText { get; init; }

        public Task<EmbeddingResponse> GenerateEmbeddingAsync(
            EmbeddingRequest request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);

            if (request.Text == ShouldFailForText)
            {
                throw new InvalidOperationException("Embedding generation failed.");
            }

            return Task.FromResult(new EmbeddingResponse
            {
                Provider = AiProviderNames.Ollama,
                Model = request.Model,
                Vector = [0.1f, 0.2f, 0.3f]
            });
        }
    }

    private sealed class FakeVectorMemoryStore : IVectorMemoryStore
    {
        public List<VectorMemoryDocument> Documents { get; } = [];

        public Task UpsertAsync(
            VectorMemoryDocument document,
            CancellationToken cancellationToken)
        {
            Documents.Add(document);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<VectorMemorySearchResult>> SearchAsync(
            IReadOnlyList<float> queryVector,
            int limit,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task DeleteAsync(
            Guid memoryId,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeIncrementalVectorMemoryStore : IVectorMemoryStore, IVectorMemoryIndexStateStore
    {
        private readonly string? _indexedContentHash;

        public FakeIncrementalVectorMemoryStore(string? indexedContentHash)
        {
            _indexedContentHash = indexedContentHash;
        }

        public List<VectorMemoryDocument> Documents { get; } = [];

        public int ReadStateCalls { get; private set; }

        public Task<string?> TryGetIndexedContentHashAsync(
            string documentId,
            CancellationToken cancellationToken)
        {
            ReadStateCalls++;

            return Task.FromResult(_indexedContentHash);
        }

        public Task UpsertAsync(
            VectorMemoryDocument document,
            CancellationToken cancellationToken)
        {
            Documents.Add(document);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<VectorMemorySearchResult>> SearchAsync(
            IReadOnlyList<float> queryVector,
            int limit,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task DeleteAsync(
            Guid memoryId,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    #endregion
}
