using DevMemory.Application.Ai.Indexing;
using DevMemory.Core;

namespace DevMemory.Application.Tests.Ai.Indexing;

public sealed class VectorMemoryDocumentBuilderTests
{
    [Fact]
    public void BuildFromMemories_WhenMemoryIsProvided_BuildsVectorDocument()
    {
        // Arrange
        var memoryId = Guid.Parse("741bf4b6-2b81-48a5-beae-1d0208e521d2");

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
        Assert.Equal(memoryId.ToString("D"), document.DocumentId);
        Assert.False(string.IsNullOrWhiteSpace(document.ContentHash));
        Assert.Equal(64, document.ContentHash.Length);
        Assert.Equal(document.ContentHash.ToLowerInvariant(), document.ContentHash);

        Assert.Equal("Estimate revision cloning", document.Title);
        Assert.Equal("LogicalCommon", document.Project);
        Assert.Equal("Estimate", document.Area);
        Assert.Equal("feature/revisions", document.Branch);
        Assert.Equal(["dotnet", "mongodb"], document.Tags);
        Assert.Equal(["EstimateService.cs"], document.FilesTouched);
        Assert.Empty(document.Vector);

        Assert.Contains("Title:", document.Text, StringComparison.Ordinal);
        Assert.Contains("Estimate revision cloning", document.Text, StringComparison.Ordinal);
        Assert.Contains("Project:", document.Text, StringComparison.Ordinal);
        Assert.Contains("LogicalCommon", document.Text, StringComparison.Ordinal);
        Assert.Contains("Area:", document.Text, StringComparison.Ordinal);
        Assert.Contains("Estimate", document.Text, StringComparison.Ordinal);
        Assert.Contains("Branch:", document.Text, StringComparison.Ordinal);
        Assert.Contains("feature/revisions", document.Text, StringComparison.Ordinal);
        Assert.Contains("Tags:", document.Text, StringComparison.Ordinal);
        Assert.Contains("- dotnet", document.Text, StringComparison.Ordinal);
        Assert.Contains("- mongodb", document.Text, StringComparison.Ordinal);
        Assert.Contains("Problem:", document.Text, StringComparison.Ordinal);
        Assert.Contains("Clone revision failed because a collection was null.", document.Text, StringComparison.Ordinal);
        Assert.Contains("Solution:", document.Text, StringComparison.Ordinal);
        Assert.Contains("Normalize null collections before deep cloning.", document.Text, StringComparison.Ordinal);
        Assert.Contains("Decisions:", document.Text, StringComparison.Ordinal);
        Assert.Contains("- Keep backward compatibility.", document.Text, StringComparison.Ordinal);
        Assert.Contains("- Do not change API contract.", document.Text, StringComparison.Ordinal);
        Assert.Contains("Files touched:", document.Text, StringComparison.Ordinal);
        Assert.Contains("- EstimateService.cs", document.Text, StringComparison.Ordinal);
        Assert.Contains("Tests:", document.Text, StringComparison.Ordinal);
        Assert.Contains("- CloneRevision_WhenCollectionsAreNull_DoesNotFail", document.Text, StringComparison.Ordinal);
        Assert.Contains("Lessons learned:", document.Text, StringComparison.Ordinal);
        Assert.Contains("Defensive initialization is important before deep clone.", document.Text, StringComparison.Ordinal);
        Assert.Contains("Created at:", document.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildFromMemories_WhenCalledTwiceForSameMemory_ProducesSameDocumentIdAndContentHash()
    {
        // Arrange
        var memory = new TaskMemory
        {
            Id = Guid.Parse("39478526-4706-455e-9444-e18d01771240"),
            Title = "MongoDB mapping fix",
            Project = "LogicalCommon",
            Area = "Nominatives",
            Branch = "feature/mapping",
            Solution = "Fixed the Mapster mapping configuration.",
            CreatedAt = new DateTime(2026, 5, 23, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var firstDocument = Assert.Single(VectorMemoryDocumentBuilder.BuildFromMemories([memory]));
        var secondDocument = Assert.Single(VectorMemoryDocumentBuilder.BuildFromMemories([memory]));

        // Assert
        Assert.Equal(firstDocument.MemoryId, secondDocument.MemoryId);
        Assert.Equal(firstDocument.DocumentId, secondDocument.DocumentId);
        Assert.Equal(firstDocument.ContentHash, secondDocument.ContentHash);
        Assert.Equal(firstDocument.Text, secondDocument.Text);
    }

    [Fact]
    public void BuildFromMemories_WhenMemoryContentChanges_ProducesSameDocumentIdAndDifferentContentHash()
    {
        // Arrange
        var memoryId = Guid.Parse("c4481c81-4d7e-4033-abb5-a16e30748bf3");

        var firstMemory = new TaskMemory
        {
            Id = memoryId,
            Title = "Indexing",
            Project = "DevMemory",
            Area = "AI",
            Solution = "Initial solution.",
            CreatedAt = new DateTime(2026, 5, 23, 10, 0, 0, DateTimeKind.Utc)
        };

        var secondMemory = new TaskMemory
        {
            Id = memoryId,
            Title = "Indexing",
            Project = "DevMemory",
            Area = "AI",
            Solution = "Updated solution.",
            CreatedAt = new DateTime(2026, 5, 23, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var firstDocument = Assert.Single(VectorMemoryDocumentBuilder.BuildFromMemories([firstMemory]));
        var secondDocument = Assert.Single(VectorMemoryDocumentBuilder.BuildFromMemories([secondMemory]));

        // Assert
        Assert.Equal(firstDocument.MemoryId, secondDocument.MemoryId);
        Assert.Equal(firstDocument.DocumentId, secondDocument.DocumentId);
        Assert.NotEqual(firstDocument.ContentHash, secondDocument.ContentHash);
        Assert.NotEqual(firstDocument.Text, secondDocument.Text);
    }

    [Fact]
    public void BuildFromMemories_WhenMemoryIdIsSame_DocumentIdUsesStableGuidFormat()
    {
        // Arrange
        var memoryId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        var memory = new TaskMemory
        {
            Id = memoryId,
            Title = "Stable identity",
            Project = "DevMemory",
            Area = "AI",
            Solution = "Use the memory id as stable vector document id.",
            CreatedAt = new DateTime(2026, 5, 23, 10, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var document = Assert.Single(VectorMemoryDocumentBuilder.BuildFromMemories([memory]));

        // Assert
        Assert.Equal("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", document.DocumentId);
    }
}
