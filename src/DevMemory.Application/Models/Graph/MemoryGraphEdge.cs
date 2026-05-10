namespace DevMemory.Application.Models.Graph;

public sealed class MemoryGraphEdge
{
    public required string SourceId { get; init; }

    public required string TargetId { get; init; }

    public required string Type { get; init; }
}
