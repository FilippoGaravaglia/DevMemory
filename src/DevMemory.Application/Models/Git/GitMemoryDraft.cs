using DevMemory.Core;

namespace DevMemory.Application.Models;

public sealed class GitMemoryDraft
{
    public required TaskMemory Memory { get; init; }

    public required GitRepositorySnapshot Snapshot { get; init; }
}
