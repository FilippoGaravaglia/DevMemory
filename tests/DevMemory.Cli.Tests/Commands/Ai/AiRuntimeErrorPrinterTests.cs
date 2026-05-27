using System.Globalization;
using DevMemory.Cli.Commands.Ai;
using DevMemory.Cli.Tests.TestSupport;

namespace DevMemory.Cli.Tests.Commands.Ai;

[Collection(CliTestCollections.ConsoleOutput)]
public sealed class AiRuntimeErrorPrinterTests
{
    [Fact]
    public void PrintFailure_WhenConnectionIsRefused_PrintsActionableGuidance()
    {
        // Arrange
        var exception = new HttpRequestException("Connection refused (localhost:6333)");

        // Act
        var error = CaptureError(() =>
            AiRuntimeErrorPrinter.PrintFailure("Semantic search", exception));

        // Assert
        Assert.Contains("Semantic search failed.", error, StringComparison.Ordinal);
        Assert.Contains("The configured AI runtime service is not reachable.", error, StringComparison.Ordinal);
        Assert.Contains("Connection refused (localhost:6333)", error, StringComparison.Ordinal);
        Assert.Contains("./scripts/dev-ai-local.sh doctor", error, StringComparison.Ordinal);
        Assert.Contains("./scripts/dev-ai-local.sh start", error, StringComparison.Ordinal);
        Assert.Contains("./scripts/dev-ai-local.sh pull-models", error, StringComparison.Ordinal);
    }

    [Fact]
    public void PrintFailure_WhenTimeoutOccurs_PrintsTimeoutGuidance()
    {
        // Arrange
        var exception = new TaskCanceledException("The request timed out.");

        // Act
        var error = CaptureError(() =>
            AiRuntimeErrorPrinter.PrintFailure("RAG answer request", exception));

        // Assert
        Assert.Contains("RAG answer request failed.", error, StringComparison.Ordinal);
        Assert.Contains("The configured AI runtime service did not respond in time.", error, StringComparison.Ordinal);
        Assert.Contains("The request timed out.", error, StringComparison.Ordinal);
        Assert.Contains("./scripts/dev-ai-local.sh doctor", error, StringComparison.Ordinal);
    }

    [Fact]
    public void PrintFailure_WhenGenericExceptionOccurs_PrintsOriginalMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("Unexpected provider error.");

        // Act
        var error = CaptureError(() =>
            AiRuntimeErrorPrinter.PrintFailure("Memory vector indexing", exception));

        // Assert
        Assert.Contains("Memory vector indexing failed.", error, StringComparison.Ordinal);
        Assert.Contains("Unexpected provider error.", error, StringComparison.Ordinal);
    }

    /// <summary>
    /// Captures standard error for assertions.
    /// </summary>
    private static string CaptureError(Action action)
    {
        var originalError = Console.Error;

        using var errorWriter = new StringWriter(CultureInfo.InvariantCulture);

        try
        {
            Console.SetError(errorWriter);

            action();

            return errorWriter.ToString();
        }
        finally
        {
            Console.SetError(originalError);
        }
    }
}
