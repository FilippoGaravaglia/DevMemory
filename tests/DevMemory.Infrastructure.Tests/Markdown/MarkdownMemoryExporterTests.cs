using DevMemory.Application.Abstractions.Memory;
using DevMemory.Core;
using DevMemory.Infrastructure;
using DevMemory.Infrastructure.Markdown;
using DevMemory.Infrastructure.Storage;

namespace DevMemory.Infrastructure.Tests.Markdown;

public sealed class MarkdownMemoryExporterTests
{
    [Fact]
    public void Export_WhenMemoryIsProvided_CreatesMarkdownFile()
    {
        // Arrange
        using var tempDirectory = new TemporaryDirectory();

        var options = new DevMemoryStorageOptions
        {
            StorageDirectory = tempDirectory.Path
        };

        var exporter = new MarkdownMemoryExporter(options);

        var memory = new TaskMemory
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Title = "Fix legacy revision root",
            Project = "LogicalCommon",
            Area = "Estimate Revision",
            Branch = "TGS-500_fix_revision_options",
            Tags = ["dotnet", "unit-test"],
            Problem = "Legacy root was not detected correctly.",
            Solution = "Updated revision chain handling.",
            Decisions = ["Preserve response compatibility"],
            FilesTouched = ["EstimateRevisionService.cs"],
            Tests = ["GetRevisionOptionsAsync_WhenCurrentEstimateHasNoRevisionMetadata_TreatsItAsLegacyRoot"],
            LessonsLearned = "Legacy estimates can miss revision metadata."
        };

        // Act
        var filePath = exporter.Export(memory);

        // Assert
        Assert.True(File.Exists(filePath));

        var markdown = File.ReadAllText(filePath);

        Assert.Contains("# Fix legacy revision root", markdown);
        Assert.Contains("## Metadata", markdown);
        Assert.Contains("LogicalCommon", markdown);
        Assert.Contains("## Continuation prompt", markdown);
        Assert.Contains("Preserve response compatibility", markdown);
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

    [Fact]
    public void Delete_WhenMarkdownFileExists_RemovesMarkdownFile()
    {
        // Arrange
        using var tempDirectory = new TemporaryDirectory();

        var options = new DevMemoryStorageOptions
        {
            StorageDirectory = tempDirectory.Path
        };

        var exporter = new MarkdownMemoryExporter(options);

        var memory = new TaskMemory
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Title = "Fix legacy revision root",
            Project = "LogicalCommon",
            Area = "Estimate Revision",
            Branch = "TGS-500_fix_revision_options",
            Tags = ["dotnet", "unit-test"],
            Problem = "Legacy root was not detected correctly.",
            Solution = "Updated revision chain handling.",
            Decisions = ["Preserve response compatibility"],
            FilesTouched = ["EstimateRevisionService.cs"],
            Tests = ["GetRevisionOptionsAsync_WhenCurrentEstimateHasNoRevisionMetadata_TreatsItAsLegacyRoot"],
            LessonsLearned = "Legacy estimates can miss revision metadata."
        };

        var filePath = exporter.Export(memory);

        Assert.True(File.Exists(filePath));

        // Act
        exporter.Delete(memory);

        // Assert
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void Delete_WhenMarkdownFileDoesNotExist_DoesNotThrow()
    {
        // Arrange
        using var tempDirectory = new TemporaryDirectory();

        var options = new DevMemoryStorageOptions
        {
            StorageDirectory = tempDirectory.Path
        };

        var exporter = new MarkdownMemoryExporter(options);

        var memory = new TaskMemory
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Title = "Fix legacy revision root",
            Project = "LogicalCommon",
            Area = "Estimate Revision",
            Branch = "TGS-500_fix_revision_options",
            Tags = ["dotnet", "unit-test"],
            Problem = "Legacy root was not detected correctly.",
            Solution = "Updated revision chain handling.",
            Decisions = ["Preserve response compatibility"],
            FilesTouched = ["EstimateRevisionService.cs"],
            Tests = ["GetRevisionOptionsAsync_WhenCurrentEstimateHasNoRevisionMetadata_TreatsItAsLegacyRoot"],
            LessonsLearned = "Legacy estimates can miss revision metadata."
        };

        // Act
        var exception = Record.Exception(() => exporter.Delete(memory));

        // Assert
        Assert.Null(exception);
    }
}
