namespace DevMemory.Application.Models.Ai;

/// <summary>
/// Represents a vector search result returned by a vector memory store.
/// </summary>
public sealed record VectorMemorySearchResult
{
    public Guid MemoryId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Project { get; init; } = string.Empty;

    public string Area { get; init; } = string.Empty;

    public string Text { get; init; } = string.Empty;

    public decimal Score { get; init; }
}
