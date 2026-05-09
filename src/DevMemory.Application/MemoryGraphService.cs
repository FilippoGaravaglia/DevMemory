using DevMemory.Application.Abstractions;
using DevMemory.Application.Models.Graph;
using DevMemory.Core;
using DevMemory.Application.Filtering;

namespace DevMemory.Application;

public sealed class MemoryGraphService
{
    private readonly IMemoryRepository _repository;
    private readonly IMemoryGraphExporter _graphExporter;
    private readonly IMemoryGraphHtmlExporter _graphHtmlExporter;

    public MemoryGraphService(
        IMemoryRepository repository,
        IMemoryGraphExporter graphExporter,
        IMemoryGraphHtmlExporter graphHtmlExporter)
    {
        _repository = repository;
        _graphExporter = graphExporter;
        _graphHtmlExporter = graphHtmlExporter;
    }

    public MemoryGraphExportResult ExportGraph(string? outputPath = null)
    {
        var graph = BuildGraph(_repository.Load());
        var filePath = _graphExporter.Export(graph, outputPath);

        return new MemoryGraphExportResult
        {
            FilePath = filePath,
            NodesCount = graph.Nodes.Count,
            EdgesCount = graph.Edges.Count
        };
    }

    public MemoryGraphExportResult ExportGraphView(string? outputPath = null)
    {
        var graph = BuildGraph(_repository.Load());
        var filePath = _graphHtmlExporter.Export(graph, outputPath);

        return new MemoryGraphExportResult
        {
            FilePath = filePath,
            NodesCount = graph.Nodes.Count,
            EdgesCount = graph.Edges.Count
        };
    }

    private static MemoryGraph BuildGraph(IReadOnlyCollection<TaskMemory> memories)
    {
        var nodes = new Dictionary<string, MemoryGraphNode>(StringComparer.OrdinalIgnoreCase);
        var edgeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var graphEdges = new List<MemoryGraphEdge>();

        foreach (var memory in memories)
        {
            var memoryNodeId = $"memory:{memory.Id}";

            AddNode(nodes, memoryNodeId, memory.Title, "memory");

            if (!string.IsNullOrWhiteSpace(memory.Project))
            {
                var projectNodeId = BuildTypedNodeId("project", memory.Project);
                AddNode(nodes, projectNodeId, memory.Project, "project");
                AddEdge(edgeKeys, graphEdges, memoryNodeId, projectNodeId, "belongs_to_project");
            }

            if (!string.IsNullOrWhiteSpace(memory.Area))
            {
                var areaNodeId = BuildTypedNodeId("area", memory.Area);
                AddNode(nodes, areaNodeId, memory.Area, "area");
                AddEdge(edgeKeys, graphEdges, memoryNodeId, areaNodeId, "belongs_to_area");
            }

            foreach (var tag in memory.Tags.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                var tagNodeId = BuildTypedNodeId("tag", tag);
                AddNode(nodes, tagNodeId, tag, "tag");
                AddEdge(edgeKeys, graphEdges, memoryNodeId, tagNodeId, "has_tag");
            }

            foreach (var file in MemoryFileFilter.Filter(memory.FilesTouched))
            {
                var fileNodeId = BuildTypedNodeId("file", file);
                AddNode(nodes, fileNodeId, file, "file");
                AddEdge(edgeKeys, graphEdges, memoryNodeId, fileNodeId, "touches_file");
            }
        }

        return new MemoryGraph
        {
            Nodes = nodes.Values
                .OrderBy(node => node.Type)
                .ThenBy(node => node.Label)
                .ToList(),
            Edges = graphEdges
                .OrderBy(edge => edge.SourceId)
                .ThenBy(edge => edge.TargetId)
                .ThenBy(edge => edge.Type)
                .ToList()
        };
    }

    private static void AddNode(
        Dictionary<string, MemoryGraphNode> nodes,
        string id,
        string label,
        string type)
    {
        if (nodes.ContainsKey(id))
        {
            return;
        }

        nodes[id] = new MemoryGraphNode
        {
            Id = id,
            Label = label,
            Type = type
        };
    }

    private static void AddEdge(
        HashSet<string> edgeKeys,
        List<MemoryGraphEdge> edges,
        string sourceId,
        string targetId,
        string type)
    {
        var key = $"{sourceId}|{targetId}|{type}";

        if (!edgeKeys.Add(key))
        {
            return;
        }

        edges.Add(new MemoryGraphEdge
        {
            SourceId = sourceId,
            TargetId = targetId,
            Type = type
        });
    }

    private static string BuildTypedNodeId(string type, string value)
    {
        return $"{type}:{NormalizeId(value)}";
    }

    private static string NormalizeId(string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .Replace('\\', '/');
    }
}