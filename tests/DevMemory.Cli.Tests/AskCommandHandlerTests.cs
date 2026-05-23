using System.Globalization;
using DevMemory.Application.Abstractions.Ai;
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
        Assert.Equal(
            "Usage: devmemory ask [--rag] [--limit <number>] <question>",
            exception.Message);
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
    public void Execute_WhenChatProviderHasNoAdapter_ReturnsFailureAndPrintsGuidance()
    {
        // Arrange
        var handler = new AskCommandHandler(
            () => new AiRuntimeOptions
            {
                Chat = new ChatProviderOptions
                {
                    Provider = AiProviderNames.OpenAi,
                    OpenAiChatModel = "gpt-test"
                }
            },
            _ => null);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["ask", "What", "did", "I", "change?"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains(
            "AI chat provider 'openai' is configured, but no adapter is available yet.",
            result.Error,
            StringComparison.Ordinal);
        Assert.Contains("Currently implemented chat providers: ollama", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenChatProviderIsConfigured_ReturnsAnswerFromChatService()
    {
        // Arrange
        var handler = new AskCommandHandler(
            () => new AiRuntimeOptions
            {
                Chat = new ChatProviderOptions
                {
                    Provider = AiProviderNames.Ollama,
                    OllamaChatModel = "llama3.2"
                }
            },
            _ => new FakeChatCompletionService("This is the AI answer."));

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["ask", "What", "did", "I", "change?"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Contains("DevMemory AI answer", result.Output, StringComparison.Ordinal);
        Assert.Contains("Provider: ollama", result.Output, StringComparison.Ordinal);
        Assert.Contains("Model: llama3.2", result.Output, StringComparison.Ordinal);
        Assert.Contains("Question: What did I change?", result.Output, StringComparison.Ordinal);
        Assert.Contains("Answer:", result.Output, StringComparison.Ordinal);
        Assert.Contains("This is the AI answer.", result.Output, StringComparison.Ordinal);
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

    private sealed class FakeChatCompletionService : IChatCompletionService
    {
        private readonly string _content;

        public FakeChatCompletionService(string content)
        {
            _content = content;
        }

        public Task<ChatCompletionResponse> CompleteAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new ChatCompletionResponse
            {
                Content = _content,
                Provider = AiProviderNames.Ollama,
                Model = request.Model
            });
        }
    }
}
