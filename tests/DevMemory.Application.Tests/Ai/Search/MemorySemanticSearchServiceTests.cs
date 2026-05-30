using DevMemory.Application.Abstractions;
using DevMemory.Application.Ai.Search;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;

namespace DevMemory.Application.Tests.Ai.Search;

public sealed class MemorySemanticSearchServiceTests
{
    [Fact]
    public async Task SearchAsync_WhenRequestIsValid_GeneratesEmbeddingAndSearchesVectorStore()
    {
        // Arrange
        var embeddingService = new FakeEmbeddingService();
        var vectorStore = new FakeVectorMemoryStore();

        var service = new MemorySemanticSearchService(embeddingService, vectorStore);

        // Act
        var results = await service.SearchAsync(
            "estimate revision cloning",
            "nomic-embed-text",
            5,
            CancellationToken.None);

        // Assert
        var embeddingRequest = Assert.Single(embeddingService.Requests);

        Assert.Equal("nomic-embed-text", embeddingRequest.Model);
        Assert.Equal("estimate revision cloning", embeddingRequest.Text);

        Assert.Equal([0.1f, 0.2f, 0.3f], vectorStore.LastQueryVector);
        Assert.Equal(5, vectorStore.LastLimit);

        var result = Assert.Single(results);

        Assert.Equal("Estimate revision cloning", result.Title);
        Assert.Equal("DevMemory", result.Project);
        Assert.Equal("AI", result.Area);
        Assert.Equal("Implemented estimate revision cloning.", result.Text);
        Assert.Equal(0.91m, result.Score);
    }

    [Fact]
    public async Task SearchAsync_WhenQueryIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var service = new MemorySemanticSearchService(
            new FakeEmbeddingService(),
            new FakeVectorMemoryStore());

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.SearchAsync(string.Empty, "nomic-embed-text", 5, CancellationToken.None));

        // Assert
        Assert.Contains("Semantic search query cannot be empty.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SearchAsync_WhenEmbeddingModelIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var service = new MemorySemanticSearchService(
            new FakeEmbeddingService(),
            new FakeVectorMemoryStore());

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.SearchAsync("query", string.Empty, 5, CancellationToken.None));

        // Assert
        Assert.Contains("Embedding model cannot be empty.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SearchAsync_WhenLimitIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var service = new MemorySemanticSearchService(
            new FakeEmbeddingService(),
            new FakeVectorMemoryStore());

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.SearchAsync("query", "nomic-embed-text", 0, CancellationToken.None));

        // Assert
        Assert.Contains("Semantic search limit must be greater than zero.", exception.Message, StringComparison.Ordinal);
    }

    private sealed class FakeEmbeddingService : IEmbeddingService
    {
        public List<EmbeddingRequest> Requests { get; } = [];

        public Task<EmbeddingResponse> GenerateEmbeddingAsync(
            EmbeddingRequest request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);

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
        public IReadOnlyList<float> LastQueryVector { get; private set; } = [];

        public int LastLimit { get; private set; }

        public Task UpsertAsync(
            VectorMemoryDocument document,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyCollection<VectorMemorySearchResult>> SearchAsync(
            IReadOnlyList<float> queryVector,
            int limit,
            CancellationToken cancellationToken)
        {
            LastQueryVector = queryVector;
            LastLimit = limit;

            IReadOnlyCollection<VectorMemorySearchResult> results =
            [
                new VectorMemorySearchResult
                {
                    MemoryId = Guid.NewGuid(),
                    Title = "Estimate revision cloning",
                    Project = "DevMemory",
                    Area = "AI",
                    Text = "Implemented estimate revision cloning.",
                    Score = 0.91m
                }
            ];

            return Task.FromResult(results);
        }

        public Task DeleteAsync(
            Guid memoryId,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
