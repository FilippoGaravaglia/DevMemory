namespace DevMemory.Application.Models.Graph;

public sealed class MemoryGraphExportResult
{
    public required string FilePath { get; init; }

    public int NodesCount { get; init; }

    public int EdgesCount { get; init; }
}
