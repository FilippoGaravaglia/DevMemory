using DevMemory.Application;
using DevMemory.Application.Abstractions.Memory;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands.Memory;
using DevMemory.Core;

namespace DevMemory.Cli.Tests.Commands.Memory;

public sealed class EditCommandHandlerTests
{
    [Fact]
    public void Execute_WhenMemoryIdIsMissing_ReturnsInvalidCommand()
    {
        // Arrange
        var handler = CreateHandler([]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["edit"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("Usage:", result.Error, StringComparison.Ordinal);
        Assert.Contains("devmemory edit <memory-id> [options]", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenMemoryIdIsInvalid_ReturnsInvalidCommand()
    {
        // Arrange
        var handler = CreateHandler([]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["edit", "not-a-guid", "--title", "Updated"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("Invalid memory id.", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenUnknownOptionIsProvided_ReturnsInvalidCommand()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");
        var handler = CreateHandler([CreateMemory(memoryId)]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["edit", memoryId.ToString("D"), "--unknown", "value"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("Unknown edit option: --unknown", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenRequiredOptionValueIsMissing_ReturnsInvalidCommand()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");
        var handler = CreateHandler([CreateMemory(memoryId)]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["edit", memoryId.ToString("D"), "--title"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("Option --title requires a value.", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenMemoryDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");
        var handler = CreateHandler([]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["edit", memoryId.ToString("D"), "--title", "Updated"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains($"Memory not found: {memoryId}", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenScalarOptionsAreProvided_UpdatesMemory()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");
        var repository = new TestMemoryRepository([CreateMemory(memoryId)]);
        var handler = CreateHandler(repository);

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            [
                "edit",
                memoryId.ToString("D"),
                "--title",
                "Updated title",
                "--project",
                "DevMemory",
                "--area",
                "Edit",
                "--solution",
                "Updated solution"
            ]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Memory updated successfully.", result.Output, StringComparison.Ordinal);
        Assert.Contains("Updated title", result.Output, StringComparison.Ordinal);

        var storedMemory = Assert.Single(repository.Load());
        Assert.Equal("Updated title", storedMemory.Title);
        Assert.Equal("DevMemory", storedMemory.Project);
        Assert.Equal("Edit", storedMemory.Area);
        Assert.Equal("Updated solution", storedMemory.Solution);
    }

    [Fact]
    public void Execute_WhenCollectionOptionsAreProvided_UpdatesMemoryCollections()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");
        var repository = new TestMemoryRepository([CreateMemory(memoryId)]);
        var handler = CreateHandler(repository);

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            [
                "edit",
                memoryId.ToString("D"),
                "--add-tag",
                "rag",
                "--remove-tag",
                "github",
                "--add-file",
                "EditCommandHandler.cs",
                "--add-test",
                "EditCommandHandlerTests"
            ]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        var storedMemory = Assert.Single(repository.Load());

        Assert.Contains("rag", storedMemory.Tags);
        Assert.DoesNotContain("github", storedMemory.Tags);
        Assert.Contains("EditCommandHandler.cs", storedMemory.FilesTouched);
        Assert.Contains("EditCommandHandlerTests", storedMemory.Tests);
    }

    private static EditCommandHandler CreateHandler(IReadOnlyCollection<TaskMemory> memories)
    {
        return CreateHandler(new TestMemoryRepository(memories));
    }

    private static EditCommandHandler CreateHandler(TestMemoryRepository repository)
    {
        var exporter = new TestMemoryExporter();
        var service = new MemoryService(repository, exporter);

        return new EditCommandHandler(service);
    }

    private static TaskMemory CreateMemory(Guid memoryId)
    {
        return new TaskMemory
        {
            Id = memoryId,
            Title = "Release v0.1.3 finalized",
            Project = "DevMemory",
            Area = "Release",
            Branch = "main",
            Tags = ["release", "github"],
            Problem = "Finalize release.",
            Solution = "Published GitHub release.",
            Decisions = ["Keep v0.1.3"],
            FilesTouched = ["README.md"],
            Tests = ["release-check"],
            LessonsLearned = "Keep release assets aligned.",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static CommandResult ExecuteAndCaptureOutput(
        EditCommandHandler handler,
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
