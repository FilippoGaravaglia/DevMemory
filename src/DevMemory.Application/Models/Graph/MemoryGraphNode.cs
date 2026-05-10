namespace DevMemory.Application.Models.Graph;

public sealed class MemoryGraphNode
{
    public required string Id { get; init; }

    public required string Label { get; init; }

    public required string Type { get; init; }
}
