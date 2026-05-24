
using DevMemory.Application.Models.Ai.VectorStore;

namespace DevMemory.Application.Abstractions;

/// <summary>
/// Defines a provider-independent contract for storing and searching memory vectors.
/// </summary>
public interface IVectorMemoryStore
{
    Task UpsertAsync(
        VectorMemoryDocument document,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<VectorMemorySearchResult>> SearchAsync(
        IReadOnlyList<float> queryVector,
        int limit,
        CancellationToken cancellationToken);
}
