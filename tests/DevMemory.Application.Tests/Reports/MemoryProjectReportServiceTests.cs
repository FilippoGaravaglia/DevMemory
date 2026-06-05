using DevMemory.Application.Reports;
using DevMemory.Core;

namespace DevMemory.Application.Tests.Reports;

public sealed class MemoryProjectReportServiceTests
{
    [Fact]
    public void BuildReport_WhenProjectHasNoMemories_ReturnsEmptyReport()
    {
        // Act
        var result = MemoryProjectReportService.BuildReport([], "DevMemory");

        // Assert
        Assert.Equal("DevMemory", result.Project);
        Assert.Equal(0, result.TotalMemories);
        Assert.Empty(result.Areas);
        Assert.Empty(result.Tags);
        Assert.Empty(result.FilesTouched);
        Assert.Null(result.FirstMemoryCreatedAt);
        Assert.Null(result.LastMemoryCreatedAt);

        Assert.Contains("# DevMemory project report: DevMemory", result.MarkdownContent, StringComparison.Ordinal);
        Assert.Contains("No memories were found for this project.", result.MarkdownContent, StringComparison.Ordinal);
        Assert.Contains("devmemory add", result.MarkdownContent, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildReport_WhenProjectHasMemories_ReturnsMarkdownReport()
    {
        // Arrange
        var memories = new[]
        {
            CreateMemory(
                project: "DevMemory",
                title: "Add insights command",
                area: "CLI",
                tags: ["dotnet", "insights"],
                files: ["src/DevMemory.Cli/Commands/Memory/InsightsCommandHandler.cs"],
                createdAt: new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)),
            CreateMemory(
                project: "DevMemory",
                title: "Add report command",
                area: "Reports",
                tags: ["dotnet", "markdown"],
                files: ["src/DevMemory.Application/Reports/MemoryProjectReportService.cs"],
                createdAt: new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc)),
            CreateMemory(
                project: "OtherProject",
                title: "Unrelated memory",
                area: "Other",
                tags: ["other"],
                files: ["src/Other.cs"],
                createdAt: new DateTime(2026, 6, 3, 10, 0, 0, DateTimeKind.Utc))
        };

        // Act
        var result = MemoryProjectReportService.BuildReport(memories, "DevMemory");

        // Assert
        Assert.Equal("DevMemory", result.Project);
        Assert.Equal(2, result.TotalMemories);
        Assert.Contains("CLI", result.Areas);
        Assert.Contains("Reports", result.Areas);
        Assert.Contains("dotnet", result.Tags);
        Assert.Contains("insights", result.Tags);
        Assert.Contains("markdown", result.Tags);
        Assert.Equal(new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc), result.FirstMemoryCreatedAt);
        Assert.Equal(new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc), result.LastMemoryCreatedAt);

        Assert.Contains("# DevMemory project report: DevMemory", result.MarkdownContent, StringComparison.Ordinal);
        Assert.Contains("## Summary", result.MarkdownContent, StringComparison.Ordinal);
        Assert.Contains("Total memories: 2", result.MarkdownContent, StringComparison.Ordinal);
        Assert.Contains("### 2026-06-01 — Add insights command", result.MarkdownContent, StringComparison.Ordinal);
        Assert.Contains("### 2026-06-02 — Add report command", result.MarkdownContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Unrelated memory", result.MarkdownContent, StringComparison.Ordinal);
        Assert.Contains("## Decision summary", result.MarkdownContent, StringComparison.Ordinal);
        Assert.Contains("## Lessons learned summary", result.MarkdownContent, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildReport_WhenProjectDiffersOnlyByCase_IncludesMatchingMemories()
    {
        // Arrange
        var memories = new[]
        {
            CreateMemory(
                project: "devmemory",
                title: "Lowercase project",
                area: "CLI",
                tags: ["dotnet"],
                files: ["src/A.cs"],
                createdAt: new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc))
        };

        // Act
        var result = MemoryProjectReportService.BuildReport(memories, "DevMemory");

        // Assert
        Assert.Equal(1, result.TotalMemories);
        Assert.Contains("Lowercase project", result.MarkdownContent, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildReport_WhenProjectIsMissing_ThrowsArgumentException()
    {
        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            MemoryProjectReportService.BuildReport([], " "));

        // Assert
        Assert.Equal("project", exception.ParamName);
    }

    /// <summary>
    /// Creates a test memory.
    /// </summary>
    private static TaskMemory CreateMemory(
        string project,
        string title,
        string area,
        IReadOnlyList<string> tags,
        IReadOnlyList<string> files,
        DateTime createdAt)
    {
        return new TaskMemory
        {
            Id = Guid.NewGuid(),
            Title = title,
            Project = project,
            Area = area,
            Branch = "main",
            Tags = tags.ToList(),
            Problem = $"Problem for {title}",
            Solution = $"Solution for {title}",
            Decisions = [$"Decision for {title}"],
            FilesTouched = files.ToList(),
            Tests = [$"Tests for {title}"],
            LessonsLearned = $"Lesson for {title}",
            CreatedAt = createdAt
        };
    }
}
