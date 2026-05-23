using DevMemory.Application.Ai;
using DevMemory.Core;

namespace DevMemory.Application.Tests;

public sealed class VectorMemoryDocumentBuilderTests
{
    [Fact]
    public void BuildFromMemories_WhenMemoryIsProvided_BuildsVectorDocument()
    {
        // Arrange
        var memoryId = Guid.NewGuid();

        var memories = new[]
        {
            new TaskMemory
            {
                Id = memoryId,
                Title = "Estimate revision cloning",
                Project = "LogicalCommon",
                Area = "Estimate",
                Branch = "feature/revisions",
                Tags = ["dotnet", "mongodb"],
                Problem = "Clone revision failed because a collection was null.",
                Solution = "Normalize null collections before deep cloning.",
                Decisions = ["Keep backward compatibility.", "Do not change API contract."],
                FilesTouched = ["EstimateService.cs"],
                Tests = ["CloneRevision_WhenCollectionsAreNull_DoesNotFail"],
                LessonsLearned = "Defensive initialization is important before deep clone.",
                CreatedAt = new DateTime(2026, 5, 23, 10, 0, 0, DateTimeKind.Utc)
            }
        };

        // Act
        var documents = VectorMemoryDocumentBuilder.BuildFromMemories(memories);

        // Assert
        var document = Assert.Single(documents);

        Assert.Equal(memoryId, document.MemoryId);
        Assert.Equal("Estimate revision cloning", document.Title);
        Assert.Equal("LogicalCommon", document.Project);
        Assert.Equal("Estimate", document.Area);
        Assert.Equal("feature/revisions", document.Branch);
        Assert.Equal(["dotnet", "mongodb"], document.Tags);
        Assert.Equal(["EstimateService.cs"], document.FilesTouched);
        Assert.Empty(document.Vector);

        Assert.Contains("Title:", document.Text, StringComparison.Ordinal);
        Assert.Contains("Estimate revision cloning", document.Text, StringComparison.Ordinal);
        Assert.Contains("Problem:", document.Text, StringComparison.Ordinal);
        Assert.Contains("Clone revision failed because a collection was null.", document.Text, StringComparison.Ordinal);
        Assert.Contains("Solution:", document.Text, StringComparison.Ordinal);
        Assert.Contains("Normalize null collections before deep cloning.", document.Text, StringComparison.Ordinal);
        Assert.Contains("Decisions:", document.Text, StringComparison.Ordinal);
        Assert.Contains("- Keep backward compatibility.", document.Text, StringComparison.Ordinal);
        Assert.Contains("Files touched:", document.Text, StringComparison.Ordinal);
        Assert.Contains("- EstimateService.cs", document.Text, StringComparison.Ordinal);
    }
}
