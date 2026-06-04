using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands.System;

namespace DevMemory.Cli.Tests.Commands.System;

public sealed class HelpCommandHandlerTests
{
    [Fact]
    public void Execute_WhenNoHelpTopicIsProvided_PrintsGeneralHelp()
    {
        // Arrange
        var handler = new HelpCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["help"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory - Local Developer Memory", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory help [command]", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory setup [--wizard|--next|--checklist|--local-ai|--demo|--check]", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory help setup", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory help config", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory help ask", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory help index", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenHelpAliasIsProvided_PrintsGeneralHelp()
    {
        // Arrange
        var handler = new HelpCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["help", "--help"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory - Local Developer Memory", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory help [command]", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory help setup", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory help config", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory help ask", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory help index", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenShortHelpAliasIsProvided_PrintsGeneralHelp()
    {
        // Arrange
        var handler = new HelpCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["help", "-h"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory - Local Developer Memory", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory help [command]", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory help setup", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory help config", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory help ask", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory help index", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenSetupHelpTopicIsProvided_PrintsSetupHelp()
    {
        // Arrange
        var handler = new HelpCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["help", "setup"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory setup", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory setup", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory setup --wizard", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory setup --next", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory setup --checklist", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory setup --local-ai", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory setup --demo", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory setup --check", result.Output, StringComparison.Ordinal);
        Assert.Contains("The setup command is safe by default.", result.Output, StringComparison.Ordinal);
        Assert.Contains("The setup wizard does not write configuration and does not start external services.", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenConfigHelpTopicIsProvided_PrintsConfigHelp()
    {
        // Arrange
        var handler = new HelpCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["help", "config"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory config", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory config show", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory config set <key> <value>", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory config reset", result.Output, StringComparison.Ordinal);
        Assert.Contains("chat-provider", result.Output, StringComparison.Ordinal);
        Assert.Contains("embedding-provider", result.Output, StringComparison.Ordinal);
        Assert.Contains("vector-store", result.Output, StringComparison.Ordinal);
        Assert.Contains("Environment variables > ~/.devmemory/config.json > default values", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory config set chat-provider ollama", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenUnknownHelpTopicIsProvided_ReturnsInvalidCommand()
    {
        // Arrange
        var handler = new HelpCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["help", "unknown"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);

        Assert.Contains("Unknown help topic: unknown", result.Error, StringComparison.Ordinal);
        Assert.Contains("devmemory help", result.Error, StringComparison.Ordinal);
        Assert.Contains("devmemory help setup", result.Error, StringComparison.Ordinal);
        Assert.Contains("devmemory help config", result.Error, StringComparison.Ordinal);
        Assert.Contains("devmemory help ask", result.Error, StringComparison.Ordinal);
        Assert.Contains("devmemory help index", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenAskHelpTopicIsProvided_PrintsAskHelp()
    {
        // Arrange
        var handler = new HelpCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["help", "ask"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory ask", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory ask <question>", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory ask --rag <question>", result.Output, StringComparison.Ordinal);
        Assert.Contains("--show-context", result.Output, StringComparison.Ordinal);
        Assert.Contains("--limit", result.Output, StringComparison.Ordinal);
        Assert.Contains("Basic ask requires a configured chat provider.", result.Output, StringComparison.Ordinal);
        Assert.Contains("RAG requires a chat provider, an embedding provider, a vector store and indexed memories.", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenIndexHelpTopicIsProvided_PrintsIndexHelp()
    {
        // Arrange
        var handler = new HelpCommandHandler();

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["help", "index"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory index", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory index --dry-run", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory index --force", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory index --limit <number>", result.Output, StringComparison.Ordinal);
        Assert.Contains("--show-text", result.Output, StringComparison.Ordinal);
        Assert.Contains("The local JSON storage remains the source of truth.", result.Output, StringComparison.Ordinal);
        Assert.Contains("Dry-run indexing does not require Ollama or Qdrant.", result.Output, StringComparison.Ordinal);
    }

    #region Helpers

    private static CommandExecutionResult ExecuteAndCaptureOutput(
        HelpCommandHandler handler,
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

    #endregion
}
