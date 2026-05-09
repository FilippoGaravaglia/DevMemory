namespace DevMemory.Application.Models;

public sealed class MemorySearchOptions
{
    public string Query { get; init; } = string.Empty;

    public string? Project { get; init; }

    public string? Area { get; init; }

    public string? Tag { get; init; }
}