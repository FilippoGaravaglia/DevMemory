using DevMemory.Application.Abstractions;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.VectorStore;

namespace DevMemory.Application.Ai.Search;

/// <summary>
/// Performs semantic search over indexed memory vectors.
/// </summary>
public sealed class MemorySemanticSearchService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorMemoryStore _vectorMemoryStore;

    public MemorySemanticSearchService(
        IEmbeddingService embeddingService,
        IVectorMemoryStore vectorMemoryStore)
    {
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
        _vectorMemoryStore = vectorMemoryStore ?? throw new ArgumentNullException(nameof(vectorMemoryStore));
    }

    public async Task<IReadOnlyCollection<VectorMemorySearchResult>> SearchAsync(
        string query,
        string embeddingModel,
        int limit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Semantic search query cannot be empty.", nameof(query));
        }

        if (string.IsNullOrWhiteSpace(embeddingModel))
        {
            throw new ArgumentException("Embedding model cannot be empty.", nameof(embeddingModel));
        }

        if (limit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Semantic search limit must be greater than zero.");
        }

        var embedding = await _embeddingService.GenerateEmbeddingAsync(
            new EmbeddingRequest
            {
                Model = embeddingModel,
                Text = query.Trim()
            },
            cancellationToken);

        return await _vectorMemoryStore.SearchAsync(
            embedding.Vector,
            limit,
            cancellationToken);
    }
}
