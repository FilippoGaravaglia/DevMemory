using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands.System;

namespace DevMemory.Cli.Tests.Commands.System;

public sealed class SetupCommandHandlerTests
{
    [Fact]
    public void Execute_WhenNoOptionIsProvided_PrintsGeneralSetup()
    {
        // Arrange
        var handler = new SetupCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["setup"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory setup", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory doctor", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory add", result.Output, StringComparison.Ordinal);
        Assert.Contains("./scripts/demo-local.sh", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenLocalAiOptionIsProvided_PrintsLocalAiSetup()
    {
        // Arrange
        var handler = new SetupCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["setup", "--local-ai"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory local AI setup", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory config set chat-provider ollama", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory index", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory ask --rag", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenDemoOptionIsProvided_PrintsDemoSetup()
    {
        // Arrange
        var handler = new SetupCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["setup", "--demo"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory isolated demo", result.Output, StringComparison.Ordinal);
        Assert.Contains("./scripts/demo-local.sh", result.Output, StringComparison.Ordinal);
        Assert.Contains("DEVMEMORY_KEEP_DEMO_HOME=true", result.Output, StringComparison.Ordinal);
        Assert.Contains("docs/demo.md", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenCheckOptionIsProvided_PrintsSetupChecks()
    {
        // Arrange
        var handler = new SetupCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["setup", "--check"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory setup checks", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory version", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory doctor", result.Output, StringComparison.Ordinal);
        Assert.Contains("./scripts/release-check.sh", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenHelpOptionIsProvided_PrintsUsage()
    {
        // Arrange
        var handler = new SetupCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["setup", "--help"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Usage:", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory setup --local-ai", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenUnknownOptionIsProvided_ReturnsInvalidCommand()
    {
        // Arrange
        var handler = new SetupCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["setup", "--unknown"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);

        Assert.Contains("Unknown setup option: --unknown", result.Error, StringComparison.Ordinal);
        Assert.Contains("Usage:", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenTooManyArgumentsAreProvided_ReturnsInvalidCommand()
    {
        // Arrange
        var handler = new SetupCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["setup", "--local-ai", "--extra"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);

        Assert.Contains("Usage:", result.Error, StringComparison.Ordinal);
    }

    private static CommandResult ExecuteAndCaptureOutput(
        SetupCommandHandler handler,
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

    private sealed record CommandResult(
        int ExitCode,
        string Output,
        string Error);
}
