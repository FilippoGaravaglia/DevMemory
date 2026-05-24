
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;

namespace DevMemory.Application.Models.Git;

/// <summary>
/// Represents the result of a retrieval-augmented memory answer.
/// </summary>
public sealed record MemoryRagAnswerResult
{
    public string Question { get; init; } = string.Empty;

    public string Answer { get; init; } = string.Empty;

    public string Provider { get; init; } = AiProviderNames.None;

    public string Model { get; init; } = string.Empty;

    public int ContextItemsCount { get; init; }

    public IReadOnlyCollection<VectorMemorySearchResult> ContextResults { get; init; } = [];
}
