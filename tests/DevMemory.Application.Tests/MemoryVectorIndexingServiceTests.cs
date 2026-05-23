using DevMemory.Application.Abstractions.Ai;
using DevMemory.Application.Ai;
using DevMemory.Application.Models.Ai;

namespace DevMemory.Application.Tests;

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
        Assert.Equal(0, result.FailedDocuments);
        Assert.Empty(result.Errors);

        var indexedDocument = Assert.Single(vectorStore.Documents);

        Assert.Equal(memoryId, indexedDocument.MemoryId);
        Assert.Equal("Estimate revision cloning", indexedDocument.Title);
        Assert.Equal("DevMemory", indexedDocument.Project);
        Assert.Equal("AI", indexedDocument.Area);
        Assert.Equal([0.1f, 0.2f, 0.3f], indexedDocument.Vector);

        var embeddingRequest = Assert.Single(embeddingService.Requests);

        Assert.Equal("nomic-embed-text", embeddingRequest.Model);
        Assert.Equal("Implemented estimate revision cloning.", embeddingRequest.Text);
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
                Text = "This one fails."
            },
            new VectorMemoryDocument
            {
                MemoryId = successfulMemoryId,
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
    }
}
