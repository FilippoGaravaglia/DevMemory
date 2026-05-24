using DevMemory.Application.Ai.Rag;
using DevMemory.Application.Models.Ai.VectorStore;

namespace DevMemory.Application.Tests.Ai.Rag;

public sealed class MemoryRagPromptBuilderTests
{
    [Fact]
    public void Build_WhenContextIsAvailable_BuildsPromptWithOrderedMemoryContext()
    {
        // Arrange
        var firstMemoryId = Guid.NewGuid();
        var secondMemoryId = Guid.NewGuid();

        var contextResults = new[]
        {
            new VectorMemorySearchResult
            {
                MemoryId = firstMemoryId,
                Title = "Low score memory",
                Project = "DevMemory",
                Area = "AI",
                Text = "This is less relevant.",
                Score = 0.42m
            },
            new VectorMemorySearchResult
            {
                MemoryId = secondMemoryId,
                Title = "Estimate revision cloning",
                Project = "LogicalCommon",
                Area = "Estimate",
                Text = "We fixed revision cloning by normalizing null collections before deep cloning.",
                Score = 0.91m
            }
        };

        // Act
        var prompt = MemoryRagPromptBuilder.Build(
            "How did we fix estimate revision cloning?",
            contextResults);

        // Assert
        Assert.Equal(2, prompt.ContextItemsCount);
        Assert.Contains("You are DevMemory", prompt.SystemPrompt, StringComparison.Ordinal);
        Assert.Contains("User question:", prompt.UserPrompt, StringComparison.Ordinal);
        Assert.Contains("How did we fix estimate revision cloning?", prompt.UserPrompt, StringComparison.Ordinal);

        var highScorePosition = prompt.UserPrompt.IndexOf(
            "Estimate revision cloning",
            StringComparison.Ordinal);
        var lowScorePosition = prompt.UserPrompt.IndexOf(
            "Low score memory",
            StringComparison.Ordinal);

        Assert.True(highScorePosition < lowScorePosition);

        Assert.Contains(secondMemoryId.ToString("D"), prompt.UserPrompt, StringComparison.Ordinal);
        Assert.Contains("Project: LogicalCommon", prompt.UserPrompt, StringComparison.Ordinal);
        Assert.Contains("Area: Estimate", prompt.UserPrompt, StringComparison.Ordinal);
        Assert.Contains(
            "We fixed revision cloning by normalizing null collections before deep cloning.",
            prompt.UserPrompt,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Build_WhenContextContainsEmptyText_IgnoresEmptyContextItems()
    {
        // Arrange
        var contextResults = new[]
        {
            new VectorMemorySearchResult
            {
                MemoryId = Guid.NewGuid(),
                Title = "Empty text",
                Text = string.Empty,
                Score = 0.99m
            },
            new VectorMemorySearchResult
            {
                MemoryId = Guid.NewGuid(),
                Title = "Valid memory",
                Text = "Valid context.",
                Score = 0.50m
            }
        };

        // Act
        var prompt = MemoryRagPromptBuilder.Build("Question?", contextResults);

        // Assert
        Assert.Equal(1, prompt.ContextItemsCount);
        Assert.DoesNotContain("Empty text", prompt.UserPrompt, StringComparison.Ordinal);
        Assert.Contains("Valid memory", prompt.UserPrompt, StringComparison.Ordinal);
        Assert.Contains("Valid context.", prompt.UserPrompt, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_WhenNoContextIsAvailable_BuildsNoContextPrompt()
    {
        // Arrange

        // Act
        var prompt = MemoryRagPromptBuilder.Build("What did I change last time?", []);

        // Assert
        Assert.Equal(0, prompt.ContextItemsCount);
        Assert.Contains("No relevant memory context was found.", prompt.UserPrompt, StringComparison.Ordinal);
        Assert.Contains(
            "available memories do not contain enough information",
            prompt.UserPrompt,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Build_WhenQuestionIsMissing_ThrowsArgumentException()
    {
        // Act
        var exception = Assert.Throws<ArgumentException>(
            static () => MemoryRagPromptBuilder.Build(string.Empty, []));

        // Assert
        Assert.Contains("RAG question cannot be empty.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_WhenMaxContextItemsIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            static () => MemoryRagPromptBuilder.Build("Question?", [], 0));

        // Assert
        Assert.Contains("Max context items must be greater than zero.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Build_WhenMaxContextItemsIsProvided_LimitsContextItems()
    {
        // Arrange

        var contextResults = new[]
        {
            new VectorMemorySearchResult
            {
                MemoryId = Guid.NewGuid(),
                Title = "Memory one",
                Text = "Context one.",
                Score = 0.90m
            },
            new VectorMemorySearchResult
            {
                MemoryId = Guid.NewGuid(),
                Title = "Memory two",
                Text = "Context two.",
                Score = 0.80m
            }
        };

        // Act
        var prompt = MemoryRagPromptBuilder.Build("Question?", contextResults, maxContextItems: 1);

        // Assert
        Assert.Equal(1, prompt.ContextItemsCount);
        Assert.Contains("Memory one", prompt.UserPrompt, StringComparison.Ordinal);
        Assert.DoesNotContain("Memory two", prompt.UserPrompt, StringComparison.Ordinal);
    }
}
