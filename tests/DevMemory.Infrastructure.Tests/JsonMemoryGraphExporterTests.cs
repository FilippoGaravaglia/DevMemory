using DevMemory.Application.Models.Graph;
using DevMemory.Infrastructure;
using DevMemory.Infrastructure.Graph;

namespace DevMemory.Infrastructure.Tests;

public sealed class JsonMemoryGraphExporterTests
{
    [Fact]
    public void Export_WhenGraphIsProvided_CreatesJsonGraphFile()
    {
        // Arrange
        using var tempDirectory = new TemporaryDirectory();

        var options = new DevMemoryStorageOptions
        {
            StorageDirectory = tempDirectory.Path
        };

        var exporter = new JsonMemoryGraphExporter(options);

        var graph = new MemoryGraph
        {
            Nodes =
            [
                new MemoryGraphNode
                {
                    Id = "memory:1",
                    Label = "Test memory",
                    Type = "memory"
                }
            ],
            Edges = []
        };

        // Act
        var filePath = exporter.Export(graph);

        // Assert
        Assert.True(File.Exists(filePath));

        var json = File.ReadAllText(filePath);

        Assert.Contains("memory:1", json);
        Assert.Contains("Test memory", json);
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
