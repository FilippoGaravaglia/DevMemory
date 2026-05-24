namespace DevMemory.Application.Models.Ai.Rag;

/// <summary>
/// Represents a prompt prepared for retrieval-augmented generation.
/// </summary>
public sealed record RagPrompt
{
    public string SystemPrompt { get; init; } = string.Empty;

    public string UserPrompt { get; init; } = string.Empty;

    public int ContextItemsCount { get; init; }
}
