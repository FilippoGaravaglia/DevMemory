using DevMemory.Application.Models.Graph;
using DevMemory.Infrastructure;
using DevMemory.Infrastructure.Graph;

namespace DevMemory.Infrastructure.Tests;

public sealed class HtmlMemoryGraphExporterTests
{
    [Fact]
    public void Export_WhenGraphIsProvided_CreatesHtmlGraphViewFile()
    {
        // Arrange
        using var tempDirectory = new TemporaryDirectory();

        var options = new DevMemoryStorageOptions
        {
            StorageDirectory = tempDirectory.Path
        };

        var exporter = new HtmlMemoryGraphExporter(options);

        var graph = new MemoryGraph
        {
            Nodes =
            [
                new MemoryGraphNode
                {
                    Id = "memory:1",
                    Label = "Test memory",
                    Type = "memory"
                },
                new MemoryGraphNode
                {
                    Id = "tag:dotnet",
                    Label = "dotnet",
                    Type = "tag"
                }
            ],
            Edges =
            [
                new MemoryGraphEdge
                {
                    SourceId = "memory:1",
                    TargetId = "tag:dotnet",
                    Type = "has_tag"
                }
            ]
        };

        // Act
        var filePath = exporter.Export(graph);

        // Assert
        Assert.True(File.Exists(filePath));

        var html = File.ReadAllText(filePath);

        Assert.Contains("<!doctype html>", html);
        Assert.Contains("DevMemory Knowledge Graph", html);
        Assert.Contains("const graph =", html);
        Assert.Contains("memory:1", html);
        Assert.Contains("has_tag", html);
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"devmemory-tests-{Guid.NewGuid():N}");

            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
