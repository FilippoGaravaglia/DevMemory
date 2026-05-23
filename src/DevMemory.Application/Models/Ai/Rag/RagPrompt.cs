namespace DevMemory.Application.Models.Ai;

/// <summary>
/// Represents a prompt prepared for retrieval-augmented generation.
/// </summary>
public sealed record RagPrompt
{
    public string SystemPrompt { get; init; } = string.Empty;

    public string UserPrompt { get; init; } = string.Empty;

    public int ContextItemsCount { get; init; }
}
