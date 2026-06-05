using DevMemory.Application;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands.Memory;
using DevMemory.Infrastructure.Markdown;
using DevMemory.Infrastructure.Storage;

namespace DevMemory.Cli.Tests.Commands.Memory;

public sealed class InsightsCommandHandlerTests
{
    [Fact]
    public void Execute_WhenHelpOptionIsProvided_PrintsUsage()
    {
        // Arrange
        using var environment = TemporaryDevMemoryHome.Create();
        var handler = CreateHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["insights", "--help"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory insights", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory insights", result.Output, StringComparison.Ordinal);
        Assert.Contains("does not require AI, Ollama or Qdrant", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenUnknownOptionIsProvided_ReturnsInvalidCommand()
    {
        // Arrange
        using var environment = TemporaryDevMemoryHome.Create();
        var handler = CreateHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["insights", "--unknown"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);

        Assert.Contains("Unknown option for insights command: --unknown", result.Error, StringComparison.Ordinal);
        Assert.Contains("devmemory insights", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenNoMemoriesExist_PrintsEmptyInsights()
    {
        // Arrange
        using var environment = TemporaryDevMemoryHome.Create();
        var handler = CreateHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["insights"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory insights", result.Output, StringComparison.Ordinal);
        Assert.Contains("Total memories: 0", result.Output, StringComparison.Ordinal);
        Assert.Contains("Projects: 0", result.Output, StringComparison.Ordinal);
        Assert.Contains("Areas: 0", result.Output, StringComparison.Ordinal);
        Assert.Contains("Tags: 0", result.Output, StringComparison.Ordinal);
        Assert.Contains("Files referenced: 0", result.Output, StringComparison.Ordinal);
        Assert.Contains("No memories found.", result.Output, StringComparison.Ordinal);
        Assert.Contains("Create your first memory with `devmemory add`.", result.Output, StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates an insights command handler.
    /// </summary>
    private static InsightsCommandHandler CreateHandler()
    {
        var repository = new MemoryRepository();
        var markdownExporter = new MarkdownMemoryExporter();
        var memoryService = new MemoryService(repository, markdownExporter);

        return new InsightsCommandHandler(memoryService);
    }

    /// <summary>
    /// Executes a command and captures console output.
    /// </summary>
    private static CommandExecutionResult ExecuteAndCaptureOutput(
        InsightsCommandHandler handler,
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

            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
