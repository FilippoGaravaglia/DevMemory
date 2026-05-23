namespace DevMemory.Application.Models.Ai;

/// <summary>
/// Represents a memory document prepared for vector storage.
/// </summary>
public sealed record VectorMemoryDocument
{
    public Guid MemoryId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Project { get; init; } = string.Empty;

    public string Area { get; init; } = string.Empty;

    public string Branch { get; init; } = string.Empty;

    public IReadOnlyCollection<string> Tags { get; init; } = [];

    public IReadOnlyCollection<string> FilesTouched { get; init; } = [];

    public string Text { get; init; } = string.Empty;

    public IReadOnlyList<float> Vector { get; init; } = [];
}
