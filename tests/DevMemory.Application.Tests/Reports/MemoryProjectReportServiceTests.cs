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

    [Fact]
    public void BuildReport_WhenAreaFilterIsProvided_IncludesOnlyMatchingArea()
    {
        // Arrange
        var memories = new[]
        {
            CreateMemory("DevMemory", "AI memory", "AI", ["rag"], ["src/A.cs"], new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)),
            CreateMemory("DevMemory", "CLI memory", "CLI", ["cli"], ["src/B.cs"], new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc))
        };

        var options = new MemoryProjectReportOptions(
            Project: "DevMemory",
            Area: "AI",
            Tag: null,
            From: null,
            To: null);

        // Act
        var result = MemoryProjectReportService.BuildReport(memories, options);

        // Assert
        Assert.Equal(1, result.TotalMemories);
        Assert.Equal("AI", result.Area);
        Assert.Contains("AI memory", result.MarkdownContent, StringComparison.Ordinal);
        Assert.DoesNotContain("CLI memory", result.MarkdownContent, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildReport_WhenTagFilterIsProvided_IncludesOnlyMatchingTag()
    {
        // Arrange
        var memories = new[]
        {
            CreateMemory("DevMemory", "RAG memory", "AI", ["rag"], ["src/A.cs"], new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)),
            CreateMemory("DevMemory", "CLI memory", "CLI", ["cli"], ["src/B.cs"], new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc))
        };

        var options = new MemoryProjectReportOptions(
            Project: "DevMemory",
            Area: null,
            Tag: "rag",
            From: null,
            To: null);

        // Act
        var result = MemoryProjectReportService.BuildReport(memories, options);

        // Assert
        Assert.Equal(1, result.TotalMemories);
        Assert.Equal("rag", result.Tag);
        Assert.Contains("RAG memory", result.MarkdownContent, StringComparison.Ordinal);
        Assert.DoesNotContain("CLI memory", result.MarkdownContent, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildReport_WhenDateFiltersAreProvided_IncludesOnlyMemoriesInRange()
    {
        // Arrange
        var memories = new[]
        {
            CreateMemory("DevMemory", "Before range", "AI", ["rag"], ["src/A.cs"], new DateTime(2026, 5, 31, 10, 0, 0, DateTimeKind.Utc)),
            CreateMemory("DevMemory", "Inside range", "AI", ["rag"], ["src/B.cs"], new DateTime(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc)),
            CreateMemory("DevMemory", "After range", "AI", ["rag"], ["src/C.cs"], new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc))
        };

        var options = new MemoryProjectReportOptions(
            Project: "DevMemory",
            Area: null,
            Tag: null,
            From: new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            To: new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc));

        // Act
        var result = MemoryProjectReportService.BuildReport(memories, options);

        // Assert
        Assert.Equal(1, result.TotalMemories);
        Assert.Contains("Inside range", result.MarkdownContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Before range", result.MarkdownContent, StringComparison.Ordinal);
        Assert.DoesNotContain("After range", result.MarkdownContent, StringComparison.Ordinal);
    }

    #region Helpers

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

    #endregion
}
