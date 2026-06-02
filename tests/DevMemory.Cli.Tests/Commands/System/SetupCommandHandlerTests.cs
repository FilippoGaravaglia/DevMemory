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
        Assert.Contains("devmemory setup --checklist", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory setup --next", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory setup --wizard", result.Output, StringComparison.Ordinal);
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

    [Fact]
    public void Execute_WhenChecklistOptionIsProvided_PrintsSetupChecklist()
    {
        // Arrange
        var handler = new SetupCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["setup", "--checklist"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory first-run checklist", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory version", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory storage", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory doctor", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory graph-view", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory setup --local-ai", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenNextOptionIsProvided_PrintsRecommendedNextSteps()
    {
        // Arrange
        var handler = new SetupCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["setup", "--next"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory recommended next steps", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory doctor", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory storage", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory add", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory graph-view", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory setup --checklist", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenWizardOptionIsProvidedAndUserSkipsAi_PrintsWizardWithoutAiSteps()
    {
        // Arrange
        var handler = new SetupCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["setup", "--wizard"],
            input: "n");

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory interactive setup wizard", result.Output, StringComparison.Ordinal);
        Assert.Contains("No files will be modified.", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory doctor", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory add", result.Output, StringComparison.Ordinal);
        Assert.Contains("Local AI/RAG setup skipped.", result.Output, StringComparison.Ordinal);
        Assert.Contains("Setup wizard completed.", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenWizardOptionIsProvidedAndUserAcceptsAi_PrintsAiSetupSteps()
    {
        // Arrange
        var handler = new SetupCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["setup", "--wizard"],
            input: "y");

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory interactive setup wizard", result.Output, StringComparison.Ordinal);
        Assert.Contains("Optional local AI/RAG setup", result.Output, StringComparison.Ordinal);
        Assert.Contains("./scripts/dev-ai-local.sh pull-models", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory config set chat-provider ollama", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory ask --rag --show-context", result.Output, StringComparison.Ordinal);
        Assert.Contains("Setup wizard completed.", result.Output, StringComparison.Ordinal);
    }

    #region Helpers

    private static CommandResult ExecuteAndCaptureOutput(
        SetupCommandHandler handler,
        string[] args,
        string? input = null)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        using var inputReader = new StringReader(input ?? string.Empty);

        var originalOutput = Console.Out;
        var originalError = Console.Error;
        var originalInput = Console.In;

        try
        {
            Console.SetOut(output);
            Console.SetError(error);
            Console.SetIn(inputReader);

            var exitCode = handler.Execute(args);

            return new CommandResult(
                exitCode,
                output.ToString(),
                error.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
            Console.SetError(originalError);
            Console.SetIn(originalInput);
        }
    }

    private sealed record CommandResult(
        int ExitCode,
        string Output,
        string Error);

    #endregion
}
