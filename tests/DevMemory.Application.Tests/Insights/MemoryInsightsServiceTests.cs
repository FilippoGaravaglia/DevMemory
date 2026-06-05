using DevMemory.Application.Insights;
using DevMemory.Core;

namespace DevMemory.Application.Tests.Insights;

public sealed class MemoryInsightsServiceTests
{
    [Fact]
    public void BuildInsights_WhenNoMemories_ReturnsEmptyInsightsWithSuggestions()
    {
        // Act
        var result = MemoryInsightsService.BuildInsights([]);

        // Assert
        Assert.Equal(0, result.TotalMemories);
        Assert.Equal(0, result.ProjectCount);
        Assert.Equal(0, result.AreaCount);
        Assert.Equal(0, result.TagCount);
        Assert.Equal(0, result.FileReferenceCount);
        Assert.Empty(result.MostActiveProjects);
        Assert.Empty(result.MostCommonAreas);
        Assert.Empty(result.MostUsedTags);
        Assert.Null(result.LastMemoryCreatedAt);
        Assert.Null(result.MostActiveMonth);
        Assert.Contains("Create your first memory with `devmemory add`.", result.Suggestions);
        Assert.Contains("Run `devmemory setup --wizard` if you want guided onboarding.", result.Suggestions);
    }

    [Fact]
    public void BuildInsights_WhenMemoriesExist_ReturnsAggregatedCounts()
    {
        // Arrange
        var memories = new[]
        {
            CreateMemory(
                project: "DevMemory",
                area: "AI",
                tags: ["rag", "qdrant"],
                files: ["src/A.cs"],
                createdAt: new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)),
            CreateMemory(
                project: "DevMemory",
                area: "CLI",
                tags: ["dotnet", "cli"],
                files: ["src/B.cs", "src/A.cs"],
                createdAt: new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc)),
            CreateMemory(
                project: "LogicalCommon",
                area: "AI",
                tags: ["rag"],
                files: ["src/C.cs"],
                createdAt: new DateTime(2026, 5, 20, 10, 0, 0, DateTimeKind.Utc))
        };

        // Act
        var result = MemoryInsightsService.BuildInsights(memories);

        // Assert
        Assert.Equal(3, result.TotalMemories);
        Assert.Equal(2, result.ProjectCount);
        Assert.Equal(2, result.AreaCount);
        Assert.Equal(4, result.TagCount);
        Assert.Equal(3, result.FileReferenceCount);
        Assert.Equal(new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc), result.LastMemoryCreatedAt);
        Assert.Equal("2026-06", result.MostActiveMonth);

        Assert.Contains(result.MostActiveProjects, item => item.Name == "DevMemory" && item.Count == 2);
        Assert.Contains(result.MostActiveProjects, item => item.Name == "LogicalCommon" && item.Count == 1);
        Assert.Contains(result.MostCommonAreas, item => item.Name == "AI" && item.Count == 2);
        Assert.Contains(result.MostCommonAreas, item => item.Name == "CLI" && item.Count == 1);
        Assert.Contains(result.MostUsedTags, item => item.Name == "rag" && item.Count == 2);
        Assert.Contains(result.MostUsedTags, item => item.Name == "qdrant" && item.Count == 1);
    }

    [Fact]
    public void BuildInsights_WhenSomeMemoriesHaveNoTags_AddsMetadataSuggestion()
    {
        // Arrange
        var memories = new[]
        {
            CreateMemory(
                project: "DevMemory",
                area: "CLI",
                tags: [],
                files: [],
                createdAt: new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc))
        };

        // Act
        var result = MemoryInsightsService.BuildInsights(memories);

        // Assert
        Assert.Contains(
            "You have 1 memories without tags: consider improving metadata for better search.",
            result.Suggestions);
    }

    [Fact]
    public void BuildInsights_WhenMemoriesReferenceDuplicateFiles_CountsDistinctFilesIgnoringCase()
    {
        // Arrange
        var memories = new[]
        {
            CreateMemory(
                project: "DevMemory",
                area: "CLI",
                tags: ["dotnet"],
                files: ["src/Program.cs", "SRC/PROGRAM.CS", "src/Commands/HelpCommandHandler.cs"],
                createdAt: new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc))
        };

        // Act
        var result = MemoryInsightsService.BuildInsights(memories);

        // Assert
        Assert.Equal(2, result.FileReferenceCount);
    }

    [Fact]
    public void BuildInsights_WhenAiRelatedTagsExist_AddsIndexSuggestion()
    {
        // Arrange
        var memories = new[]
        {
            CreateMemory(
                project: "DevMemory",
                area: "AI",
                tags: ["rag"],
                files: ["src/A.cs"],
                createdAt: new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)),
            CreateMemory(
                project: "DevMemory",
                area: "AI",
                tags: ["qdrant"],
                files: ["src/B.cs"],
                createdAt: new DateTime(2026, 6, 2, 10, 0, 0, DateTimeKind.Utc)),
            CreateMemory(
                project: "DevMemory",
                area: "CLI",
                tags: ["dotnet"],
                files: ["src/C.cs"],
                createdAt: new DateTime(2026, 6, 3, 10, 0, 0, DateTimeKind.Utc))
        };

        // Act
        var result = MemoryInsightsService.BuildInsights(memories);

        // Assert
        Assert.Contains(
            "You have AI/RAG-related memories: consider running `devmemory index` for semantic search.",
            result.Suggestions);
    }

    /// <summary>
    /// Creates a test memory.
    /// </summary>
    private static TaskMemory CreateMemory(
        string project,
        string area,
        IReadOnlyList<string> tags,
        IReadOnlyList<string> files,
        DateTime createdAt)
    {
        return new TaskMemory
        {
            Id = Guid.NewGuid(),
            Title = "Test memory",
            Project = project,
            Area = area,
            Branch = "main",
            Tags = tags.ToList(),
            Problem = "Problem",
            Solution = "Solution",
            Decisions = ["Decision"],
            FilesTouched = files.ToList(),
            Tests = ["Tests"],
            LessonsLearned = "Lesson",
            CreatedAt = createdAt
        };
    }
}
