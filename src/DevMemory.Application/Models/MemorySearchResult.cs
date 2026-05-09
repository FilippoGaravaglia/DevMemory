using DevMemory.Core;

namespace DevMemory.Application.Models;

public sealed class MemorySearchResult
{
    public required TaskMemory Memory { get; init; }

    public int Score { get; init; }
}