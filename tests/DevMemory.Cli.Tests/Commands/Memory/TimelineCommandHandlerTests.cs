using DevMemory.Application;
using DevMemory.Application.Abstractions.Memory;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands.Memory;
using DevMemory.Core;

namespace DevMemory.Cli.Tests.Commands.Memory;

public sealed class TimelineCommandHandlerTests
{
    [Fact]
    public void Execute_WhenNoMemoriesExist_PrintsEmptyTimeline()
    {
        // Arrange
        var handler = CreateHandler([]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["timeline"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory timeline", result.Output, StringComparison.Ordinal);
        Assert.Contains("Results: 0", result.Output, StringComparison.Ordinal);
        Assert.Contains("No memories found for the selected timeline filters.", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenMemoriesExist_PrintsTimelineGroupedByDate()
    {
        // Arrange
        var firstMemoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");
        var secondMemoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var handler = CreateHandler(
        [
            CreateMemory(
                firstMemoryId,
                "Persistent AI configuration",
                "DevMemory",
                "AI",
                ["ollama", "qdrant"],
                new DateTime(2026, 5, 30, 10, 0, 0, DateTimeKind.Utc)),
            CreateMemory(
                secondMemoryId,
                "Release v0.1.3 finalized",
                "DevMemory",
                "Release",
                ["release"],
                new DateTime(2026, 5, 29, 12, 10, 0, DateTimeKind.Utc))
        ]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["timeline"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("2026-05-30", result.Output, StringComparison.Ordinal);
        Assert.Contains("10:00  Persistent AI configuration", result.Output, StringComparison.Ordinal);
        Assert.Contains(firstMemoryId.ToString("D"), result.Output, StringComparison.Ordinal);

        Assert.Contains("2026-05-29", result.Output, StringComparison.Ordinal);
        Assert.Contains("12:10  Release v0.1.3 finalized", result.Output, StringComparison.Ordinal);
        Assert.Contains(secondMemoryId.ToString("D"), result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenProjectFilterIsProvided_PrintsOnlyMatchingMemories()
    {
        // Arrange
        var devMemoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");
        var logicalCommonId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var handler = CreateHandler(
        [
            CreateMemory(
                devMemoryId,
                "DevMemory feature",
                "DevMemory",
                "AI",
                ["rag"],
                new DateTime(2026, 5, 30, 10, 0, 0, DateTimeKind.Utc)),
            CreateMemory(
                logicalCommonId,
                "LogicalCommon feature",
                "LogicalCommon",
                "Estimate",
                ["dotnet"],
                new DateTime(2026, 5, 29, 12, 0, 0, DateTimeKind.Utc))
        ]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["timeline", "--project", "DevMemory"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Project: DevMemory", result.Output, StringComparison.Ordinal);
        Assert.Contains("DevMemory feature", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("LogicalCommon feature", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenAreaFilterIsProvided_PrintsOnlyMatchingMemories()
    {
        // Arrange
        var handler = CreateHandler(
        [
            CreateMemory(
                Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4"),
                "AI feature",
                "DevMemory",
                "AI",
                ["rag"],
                new DateTime(2026, 5, 30, 10, 0, 0, DateTimeKind.Utc)),
            CreateMemory(
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                "Release feature",
                "DevMemory",
                "Release",
                ["release"],
                new DateTime(2026, 5, 29, 12, 0, 0, DateTimeKind.Utc))
        ]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["timeline", "--area", "AI"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Area: AI", result.Output, StringComparison.Ordinal);
        Assert.Contains("AI feature", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Release feature", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenTagFilterIsProvided_PrintsOnlyMatchingMemories()
    {
        // Arrange
        var handler = CreateHandler(
        [
            CreateMemory(
                Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4"),
                "RAG feature",
                "DevMemory",
                "AI",
                ["rag"],
                new DateTime(2026, 5, 30, 10, 0, 0, DateTimeKind.Utc)),
            CreateMemory(
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                "Release feature",
                "DevMemory",
                "Release",
                ["release"],
                new DateTime(2026, 5, 29, 12, 0, 0, DateTimeKind.Utc))
        ]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["timeline", "--tag", "rag"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Tag: rag", result.Output, StringComparison.Ordinal);
        Assert.Contains("RAG feature", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Release feature", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenLimitIsProvided_PrintsOnlyRequestedNumberOfMemories()
    {
        // Arrange
        var handler = CreateHandler(
        [
            CreateMemory(
                Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4"),
                "Newest memory",
                "DevMemory",
                "AI",
                ["rag"],
                new DateTime(2026, 5, 30, 10, 0, 0, DateTimeKind.Utc)),
            CreateMemory(
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                "Oldest memory",
                "DevMemory",
                "Release",
                ["release"],
                new DateTime(2026, 5, 29, 12, 0, 0, DateTimeKind.Utc))
        ]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["timeline", "--limit", "1"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Limit: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("Results: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("Newest memory", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Oldest memory", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenLimitIsInvalid_ReturnsInvalidCommand()
    {
        // Arrange
        var handler = CreateHandler([]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["timeline", "--limit", "0"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("Option --limit must be a positive integer.", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenUnknownOptionIsProvided_ReturnsInvalidCommand()
    {
        // Arrange
        var handler = CreateHandler([]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["timeline", "--unknown"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("Unknown timeline option: --unknown", result.Error, StringComparison.Ordinal);
    }

    private static TimelineCommandHandler CreateHandler(IReadOnlyCollection<TaskMemory> memories)
    {
        var repository = new TestMemoryRepository(memories);
        var exporter = new TestMemoryExporter();
        var memoryService = new MemoryService(repository, exporter);

        return new TimelineCommandHandler(memoryService);
    }

    private static TaskMemory CreateMemory(
        Guid id,
        string title,
        string project,
        string area,
        IReadOnlyCollection<string> tags,
        DateTime createdAt)
    {
        return new TaskMemory
        {
            Id = id,
            Title = title,
            Project = project,
            Area = area,
            Branch = "main",
            Tags = tags.ToList(),
            Problem = "Problem.",
            Solution = "Solution.",
            Decisions = ["Decision."],
            FilesTouched = ["File.cs"],
            Tests = ["Test."],
            LessonsLearned = "Lesson.",
            CreatedAt = createdAt
        };
    }

    private static CommandResult ExecuteAndCaptureOutput(
        TimelineCommandHandler handler,
        string[] args)
    {
        var originalOutput = Console.Out;
        var originalError = Console.Error;

        using var outputWriter = new StringWriter();
        using var errorWriter = new StringWriter();

        try
        {
            Console.SetOut(outputWriter);
            Console.SetError(errorWriter);

            var exitCode = handler.Execute(args);

            return new CommandResult(
                exitCode,
                outputWriter.ToString(),
                errorWriter.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
            Console.SetError(originalError);
        }
    }

    private sealed class TestMemoryRepository : IMemoryRepository
    {
        private readonly List<TaskMemory> _memories;

        public TestMemoryRepository(IReadOnlyCollection<TaskMemory> memories)
        {
            _memories = memories.ToList();
        }

        public List<TaskMemory> Load()
        {
            return _memories.ToList();
        }

        public void Save(List<TaskMemory> memories)
        {
            _memories.Clear();
            _memories.AddRange(memories);
        }

        public string GetStorageFilePath()
        {
            return "/tmp/devmemory.json";
        }

        public string GetMarkdownDirectoryPath()
        {
            return "/tmp/markdown";
        }
    }

    private sealed class TestMemoryExporter : IMemoryExporter
    {
        public string Export(TaskMemory memory)
        {
            return $"/tmp/markdown/{memory.Id:D}.md";
        }

        public void Delete(TaskMemory memory)
        {
        }
    }

    private sealed record CommandResult(
        int ExitCode,
        string Output,
        string Error);
}
