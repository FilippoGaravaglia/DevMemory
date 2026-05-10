namespace DevMemory.Application.Models.Graph;

public sealed class MemoryGraph
{
    public List<MemoryGraphNode> Nodes { get; init; } = [];

    public List<MemoryGraphEdge> Edges { get; init; } = [];
}
