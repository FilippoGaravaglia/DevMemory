using System.Globalization;
using DevMemory.Application.Models.Ai;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands;
using DevMemory.Cli.Tests.TestSupport;

namespace DevMemory.Cli.Tests;

[Collection(CliTestCollections.ConsoleOutput)]
public sealed class AskCommandHandlerTests
{
    [Fact]
    public void Execute_WhenQuestionIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var handler = new AskCommandHandler(() => new AiRuntimeOptions());

        // Act
        var exception = Assert.Throws<ArgumentException>(() => handler.Execute(["ask"]));

        // Assert
        Assert.Equal("Usage: devmemory ask <question>", exception.Message);
    }

    [Fact]
    public void Execute_WhenChatProviderIsNotConfigured_ReturnsFailureAndPrintsGuidance()
    {
        // Arrange
        var handler = new AskCommandHandler(() => new AiRuntimeOptions());

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["ask", "What", "did", "I", "change?"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("AI chat is not configured.", result.Error, StringComparison.Ordinal);
        Assert.Contains("Configure a chat provider before using this command.", result.Error, StringComparison.Ordinal);
        Assert.DoesNotContain("What did I change?", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenOllamaChatProviderIsConfigured_PrintsProviderDetailsAndReturnsFailureUntilImplemented()
    {
        // Arrange
        var handler = new AskCommandHandler(() => new AiRuntimeOptions
        {
            Chat = new ChatProviderOptions
            {
                Provider = AiProviderNames.Ollama,
                OllamaChatModel = "llama3.2"
            }
        });

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["ask", "What", "did", "I", "change?"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Contains("DevMemory AI ask", result.Output, StringComparison.Ordinal);
        Assert.Contains("Question: What did I change?", result.Output, StringComparison.Ordinal);
        Assert.Contains("Chat provider: ollama", result.Output, StringComparison.Ordinal);
        Assert.Contains("Chat model: llama3.2", result.Output, StringComparison.Ordinal);
        Assert.Contains("AI chat execution is not implemented yet.", result.Error, StringComparison.Ordinal);
    }

    /// <summary>
    /// Executes the handler and captures standard output and standard error.
    /// </summary>
    private static (int ExitCode, string Output, string Error) ExecuteAndCaptureOutput(
        AskCommandHandler handler,
        string[] args)
    {
        var originalOutput = Console.Out;
        var originalError = Console.Error;

        using var outputWriter = new StringWriter(CultureInfo.InvariantCulture);
        using var errorWriter = new StringWriter(CultureInfo.InvariantCulture);

        try
        {
            Console.SetOut(outputWriter);
            Console.SetError(errorWriter);

            var exitCode = handler.Execute(args);

            return (exitCode, outputWriter.ToString(), errorWriter.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
            Console.SetError(originalError);
        }
    }
}
