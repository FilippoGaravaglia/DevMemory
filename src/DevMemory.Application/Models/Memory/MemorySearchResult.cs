using DevMemory.Core;

namespace DevMemory.Application.Models.Memory;

public sealed class MemorySearchResult
{
    public required TaskMemory Memory { get; init; }

    public int Score { get; init; }
}
