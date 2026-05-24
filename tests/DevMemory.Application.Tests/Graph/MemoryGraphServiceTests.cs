using DevMemory.Application.Abstractions.Graph;
using DevMemory.Application.Abstractions.Memory;
using DevMemory.Application.Models.Graph;
using DevMemory.Core;

namespace DevMemory.Application.Tests.Graph;

public sealed class MemoryGraphServiceTests
{
    [Fact]
    public void ExportGraph_WhenMemoriesExist_BuildsGraphWithExpectedNodesAndEdges()
    {
        // Arrange
        var repository = new InMemoryRepository
        {
            Memories =
            [
                new TaskMemory
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Title = "Fix revision bug",
                    Project = "LogicalCommon",
                    Area = "Estimate",
                    Tags = ["dotnet", "bug-fix"],
                    FilesTouched = ["src/EstimateRevisionService.cs"]
                }
            ]
        };

        var exporter = new InMemoryGraphExporter();

        var service = new MemoryGraphService(
            repository,
            exporter,
            new InMemoryGraphHtmlExporter());

        // Act
        var result = service.ExportGraph();

        // Assert
        Assert.Equal("/fake/path/devmemory-graph.json", result.FilePath);
        Assert.NotNull(exporter.ExportedGraph);

        Assert.Contains(exporter.ExportedGraph.Nodes, node => node.Id == "memory:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        Assert.Contains(exporter.ExportedGraph.Nodes, node => node.Id == "project:logicalcommon");
        Assert.Contains(exporter.ExportedGraph.Nodes, node => node.Id == "area:estimate");
        Assert.Contains(exporter.ExportedGraph.Nodes, node => node.Id == "tag:dotnet");
        Assert.Contains(exporter.ExportedGraph.Nodes, node => node.Id == "file:src/estimaterevisionservice.cs");

        Assert.Contains(exporter.ExportedGraph.Edges, edge =>
            edge.SourceId == "memory:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" &&
            edge.TargetId == "project:logicalcommon" &&
            edge.Type == "belongs_to_project");

        Assert.Contains(exporter.ExportedGraph.Edges, edge =>
            edge.SourceId == "memory:aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" &&
            edge.TargetId == "tag:dotnet" &&
            edge.Type == "has_tag");
    }

    [Fact]
    public void ExportGraphView_WhenMemoriesExist_ExportsHtmlGraph()
    {
        // Arrange
        var repository = new InMemoryRepository
        {
            Memories =
            [
                new TaskMemory
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Title = "Create graph view",
                    Project = "DevMemory",
                    Area = "Graph",
                    Tags = ["html", "graph"]
                }
            ]
        };

        var htmlExporter = new InMemoryGraphHtmlExporter();

        var service = new MemoryGraphService(
            repository,
            new InMemoryGraphExporter(),
            htmlExporter);

        // Act
        var result = service.ExportGraphView();

        // Assert
        Assert.Equal("/fake/path/devmemory-graph.html", result.FilePath);
        Assert.NotNull(htmlExporter.ExportedGraph);
        Assert.Contains(htmlExporter.ExportedGraph.Nodes, node => node.Id == "project:devmemory");
    }

    [Fact]
    public void ExportGraph_WhenMemoryContainsGeneratedFiles_ExcludesGeneratedFilesFromGraph()
    {
        // Arrange
        var repository = new InMemoryRepository
        {
            Memories =
            [
                new TaskMemory
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    Title = "Package CLI tool",
                    Project = "DevMemory",
                    Area = "Packaging",
                    FilesTouched =
                    [
                        "src/DevMemory.Cli/Program.cs",
                        "src/DevMemory.Cli/bin/Release/net10.0/DevMemory.Cli.dll",
                        "src/DevMemory.Cli/obj/project.assets.json",
                        "artifacts/packages/DevMemory.Cli.0.1.0.nupkg"
                    ]
                }
            ]
        };

        var exporter = new InMemoryGraphExporter();

        var service = new MemoryGraphService(
            repository,
            exporter,
            new InMemoryGraphHtmlExporter());

        // Act
        service.ExportGraph();

        // Assert
        Assert.NotNull(exporter.ExportedGraph);

        Assert.Contains(exporter.ExportedGraph.Nodes, node => node.Id == "file:src/devmemory.cli/program.cs");

        Assert.DoesNotContain(exporter.ExportedGraph.Nodes, node =>
            node.Label.Contains("/bin/", StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(exporter.ExportedGraph.Nodes, node =>
            node.Label.Contains("/obj/", StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(exporter.ExportedGraph.Nodes, node =>
            node.Label.Contains("artifacts", StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(exporter.ExportedGraph.Nodes, node =>
            node.Label.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class InMemoryRepository : IMemoryRepository
    {
        public List<TaskMemory> Memories { get; init; } = [];

        public List<TaskMemory> Load()
        {
            return Memories.ToList();
        }

        public void Save(List<TaskMemory> memories)
        {
        }

        public string GetStorageFilePath()
        {
            return "/fake/path/devmemory.json";
        }

        public string GetMarkdownDirectoryPath()
        {
            return "/fake/path/markdown";
        }
    }

    private sealed class InMemoryGraphExporter : IMemoryGraphExporter
    {
        public MemoryGraph? ExportedGraph { get; private set; }

        public string Export(MemoryGraph graph, string? outputPath = null)
        {
            ExportedGraph = graph;
            return outputPath ?? "/fake/path/devmemory-graph.json";
        }
    }

    private sealed class InMemoryGraphHtmlExporter : IMemoryGraphHtmlExporter
    {
        public MemoryGraph? ExportedGraph { get; private set; }

        public string Export(MemoryGraph graph, string? outputPath = null)
        {
            ExportedGraph = graph;
            return outputPath ?? "/fake/path/devmemory-graph.html";
        }
    }
}
