namespace DevMemory.Application.Models.Ai.VectorStore;

/// <summary>
/// Represents the result of a memory vector indexing operation.
/// </summary>
public sealed record MemoryVectorIndexingResult
{
    public int TotalDocuments { get; init; }

    public int IndexedDocuments { get; init; }

    public int FailedDocuments { get; init; }

    public IReadOnlyCollection<string> Errors { get; init; } = [];
}
