using DevMemory.Application;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands.Memory;
using DevMemory.Core;
using DevMemory.Infrastructure.Markdown;
using DevMemory.Infrastructure.Storage;

namespace DevMemory.Cli.Tests.Commands.Memory;

public sealed class ReportCommandHandlerTests
{
    [Fact]
    public void Execute_WhenHelpOptionIsProvided_PrintsUsage()
    {
        // Arrange
        using var environment = TemporaryDevMemoryHome.Create();
        var handler = CreateHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["report", "--help"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory report", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory report --project <project>", result.Output, StringComparison.Ordinal);
        Assert.Contains("does not require AI, Ollama or Qdrant", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenProjectIsMissing_ReturnsInvalidCommand()
    {
        // Arrange
        using var environment = TemporaryDevMemoryHome.Create();
        var handler = CreateHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["report"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);

        Assert.Contains("Missing required option: --project <project>", result.Error, StringComparison.Ordinal);
        Assert.Contains("devmemory report --project <project>", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenProjectIsProvided_WritesMarkdownReport()
    {
        // Arrange
        using var environment = TemporaryDevMemoryHome.Create();
        var handler = CreateHandler(out var memoryService);

        memoryService.Add(CreateMemory("DevMemory", "Add report command"));

        var outputPath = Path.Combine(environment.Path, "report.md");

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["report", "--project", "DevMemory", "--output", outputPath]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory project report", result.Output, StringComparison.Ordinal);
        Assert.Contains("Project: DevMemory", result.Output, StringComparison.Ordinal);
        Assert.Contains("Memories: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains($"Output: {outputPath}", result.Output, StringComparison.Ordinal);

        Assert.True(File.Exists(outputPath));

        var markdown = File.ReadAllText(outputPath);
        Assert.Contains("# DevMemory project report: DevMemory", markdown, StringComparison.Ordinal);
        Assert.Contains("Add report command", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenOutputExistsWithoutForce_ReturnsInvalidCommand()
    {
        // Arrange
        using var environment = TemporaryDevMemoryHome.Create();
        var handler = CreateHandler(out var memoryService);

        memoryService.Add(CreateMemory("DevMemory", "Add report command"));

        var outputPath = Path.Combine(environment.Path, "report.md");
        File.WriteAllText(outputPath, "existing");

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["report", "--project", "DevMemory", "--output", outputPath]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);

        Assert.Contains("Report file already exists:", result.Error, StringComparison.Ordinal);
        Assert.Contains("Use --force to overwrite it.", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenOutputExistsWithForce_OverwritesMarkdownReport()
    {
        // Arrange
        using var environment = TemporaryDevMemoryHome.Create();
        var handler = CreateHandler(out var memoryService);

        memoryService.Add(CreateMemory("DevMemory", "Add report command"));

        var outputPath = Path.Combine(environment.Path, "report.md");
        File.WriteAllText(outputPath, "existing");

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["report", "--project", "DevMemory", "--output", outputPath, "--force"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        var markdown = File.ReadAllText(outputPath);
        Assert.Contains("# DevMemory project report: DevMemory", markdown, StringComparison.Ordinal);
        Assert.DoesNotContain("existing", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenAreaFilterIsProvided_WritesFilteredMarkdownReport()
    {
        // Arrange
        using var environment = TemporaryDevMemoryHome.Create();
        var handler = CreateHandler(out var memoryService);

        memoryService.Add(CreateMemory("DevMemory", "AI report memory", area: "AI", tags: ["rag"]));
        memoryService.Add(CreateMemory("DevMemory", "CLI report memory", area: "CLI", tags: ["cli"]));

        var outputPath = Path.Combine(environment.Path, "report.md");

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["report", "--project", "DevMemory", "--area", "AI", "--output", outputPath]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Contains("Area filter: AI", result.Output, StringComparison.Ordinal);
        Assert.Contains("Memories: 1", result.Output, StringComparison.Ordinal);

        var markdown = File.ReadAllText(outputPath);
        Assert.Contains("AI report memory", markdown, StringComparison.Ordinal);
        Assert.DoesNotContain("CLI report memory", markdown, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenDateIsInvalid_ReturnsInvalidCommand()
    {
        // Arrange
        using var environment = TemporaryDevMemoryHome.Create();
        var handler = CreateHandler();

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["report", "--project", "DevMemory", "--from", "2026/06/01"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("Invalid value for option --from: expected yyyy-MM-dd.", result.Error, StringComparison.Ordinal);
    }

    #region Helpers

    /// <summary>
    /// Creates a report command handler.
    /// </summary>
    private static ReportCommandHandler CreateHandler()
    {
        return CreateHandler(out _);
    }

    /// <summary>
    /// Creates a report command handler and returns the backing memory service.
    /// </summary>
    private static ReportCommandHandler CreateHandler(out MemoryService memoryService)
    {
        var repository = new MemoryRepository();
        var markdownExporter = new MarkdownMemoryExporter();
        memoryService = new MemoryService(repository, markdownExporter);

        return new ReportCommandHandler(memoryService);
    }

    /// <summary>
    /// Creates a test memory.
    /// </summary>
    private static TaskMemory CreateMemory(
        string project,
        string title,
        string area = "Reports",
        IReadOnlyList<string>? tags = null)
    {
        return new TaskMemory
        {
            Id = Guid.NewGuid(),
            Title = title,
            Project = project,
            Area = area,
            Branch = "main",
            Tags = tags?.ToList() ?? ["dotnet", "report"],
            Problem = "Need a project report.",
            Solution = "Generate a Markdown report from local memories.",
            Decisions = ["Keep JSON as the source of truth."],
            FilesTouched = ["src/DevMemory.Application/Reports/MemoryProjectReportService.cs"],
            Tests = ["dotnet test"],
            LessonsLearned = "Reports make memories easier to review.",
            CreatedAt = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc)
        };
    }

    /// <summary>
    /// Executes a command and captures console output.
    /// </summary>
    private static CommandExecutionResult ExecuteAndCaptureOutput(
        ReportCommandHandler handler,
        string[] args)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var originalOutput = Console.Out;
        var originalError = Console.Error;

        try
        {
            Console.SetOut(output);
            Console.SetError(error);

            var exitCode = handler.Execute(args);

            return new CommandExecutionResult(
                exitCode,
                output.ToString(),
                error.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
            Console.SetError(originalError);
        }
    }

    private sealed record CommandExecutionResult(
        int ExitCode,
        string Output,
        string Error);

    private sealed class TemporaryDevMemoryHome : IDisposable
    {
        private const string DevMemoryHomeEnvironmentVariable = "DEVMEMORY_HOME";

        private readonly string? _originalDevMemoryHome;

        private TemporaryDevMemoryHome(string path, string? originalDevMemoryHome)
        {
            Path = path;
            _originalDevMemoryHome = originalDevMemoryHome;
        }

        public string Path { get; }

        /// <summary>
        /// Creates an isolated temporary DevMemory home directory.
        /// </summary>
        public static TemporaryDevMemoryHome Create()
        {
            var originalDevMemoryHome = Environment.GetEnvironmentVariable(DevMemoryHomeEnvironmentVariable);
            var path = global::System.IO.Path.Combine(
                global::System.IO.Path.GetTempPath(),
                $"devmemory-cli-tests-{Guid.NewGuid():N}");

            global::System.IO.Directory.CreateDirectory(path);
            Environment.SetEnvironmentVariable(DevMemoryHomeEnvironmentVariable, path);

            return new TemporaryDevMemoryHome(path, originalDevMemoryHome);
        }

        /// <summary>
        /// Restores the previous DevMemory home and deletes the temporary directory.
        /// </summary>
        public void Dispose()
        {
            Environment.SetEnvironmentVariable(DevMemoryHomeEnvironmentVariable, _originalDevMemoryHome);

            if (global::System.IO.Directory.Exists(Path))
            {
                global::System.IO.Directory.Delete(Path, recursive: true);
            }
        }
    }

    #endregion
}
