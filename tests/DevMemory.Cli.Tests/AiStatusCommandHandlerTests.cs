using System.Globalization;
using DevMemory.Application.Models.Ai;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands;

namespace DevMemory.Cli.Tests;

public sealed class AiStatusCommandHandlerTests
{
    [Fact]
    public void Execute_WhenAiIsDisabled_PrintsDisabledStatus()
    {
        // Arrange
        var handler = new AiStatusCommandHandler(() => new AiRuntimeOptions());

        // Act
        var (exitCode, output) = ExecuteAndCaptureOutput(handler);

        // Assert
        Assert.Equal(CliExitCodes.Success, exitCode);
        Assert.Contains("AI runtime status", output, StringComparison.Ordinal);
        Assert.Contains("Chat provider: none", output, StringComparison.Ordinal);
        Assert.Contains("Embedding provider: none", output, StringComparison.Ordinal);
        Assert.Contains("Vector store: none", output, StringComparison.Ordinal);
        Assert.Contains("Chat enabled: no", output, StringComparison.Ordinal);
        Assert.Contains("Semantic search enabled: no", output, StringComparison.Ordinal);
        Assert.Contains("Full RAG enabled: no", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenOllamaAndQdrantAreConfigured_PrintsLocalRagStatus()
    {
        // Arrange
        var handler = new AiStatusCommandHandler(() => new AiRuntimeOptions
        {
            Chat = new ChatProviderOptions
            {
                Provider = AiProviderNames.Ollama,
                OllamaEndpoint = "http://localhost:11434",
                OllamaChatModel = "llama3.2"
            },
            Embedding = new EmbeddingProviderOptions
            {
                Provider = AiProviderNames.Ollama,
                OllamaEndpoint = "http://localhost:11434",
                OllamaEmbeddingModel = "nomic-embed-text"
            },
            VectorStore = new VectorStoreOptions
            {
                Provider = VectorStoreNames.Qdrant,
                QdrantEndpoint = "http://localhost:6333",
                QdrantCollection = "devmemory_memories"
            }
        });

        // Act
        var (exitCode, output) = ExecuteAndCaptureOutput(handler);

        // Assert
        Assert.Equal(CliExitCodes.Success, exitCode);
        Assert.Contains("Chat provider: ollama", output, StringComparison.Ordinal);
        Assert.Contains("Chat endpoint: http://localhost:11434", output, StringComparison.Ordinal);
        Assert.Contains("Chat model: llama3.2", output, StringComparison.Ordinal);
        Assert.Contains("Embedding provider: ollama", output, StringComparison.Ordinal);
        Assert.Contains("Embedding endpoint: http://localhost:11434", output, StringComparison.Ordinal);
        Assert.Contains("Embedding model: nomic-embed-text", output, StringComparison.Ordinal);
        Assert.Contains("Vector store: qdrant", output, StringComparison.Ordinal);
        Assert.Contains("Qdrant endpoint: http://localhost:6333", output, StringComparison.Ordinal);
        Assert.Contains("Qdrant collection: devmemory_memories", output, StringComparison.Ordinal);
        Assert.Contains("Chat enabled: yes", output, StringComparison.Ordinal);
        Assert.Contains("Semantic search enabled: yes", output, StringComparison.Ordinal);
        Assert.Contains("Full RAG enabled: yes", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenCloudProviderIsConfigured_DoesNotPrintApiKeyValue()
    {
        // Arrange
        var handler = new AiStatusCommandHandler(() => new AiRuntimeOptions
        {
            Chat = new ChatProviderOptions
            {
                Provider = AiProviderNames.OpenAi,
                OpenAiApiKey = "secret-api-key",
                OpenAiChatModel = "gpt-test"
            }
        });

        // Act
        var (exitCode, output) = ExecuteAndCaptureOutput(handler);

        // Assert
        Assert.Equal(CliExitCodes.Success, exitCode);
        Assert.Contains("Chat provider: openai", output, StringComparison.Ordinal);
        Assert.Contains("Chat model: gpt-test", output, StringComparison.Ordinal);
        Assert.Contains("OpenAI API key configured: yes", output, StringComparison.Ordinal);
        Assert.DoesNotContain("secret-api-key", output, StringComparison.Ordinal);
    }

    /// <summary>
    /// Executes the handler and captures standard output.
    /// </summary>
    private static (int ExitCode, string Output) ExecuteAndCaptureOutput(AiStatusCommandHandler handler)
    {
        var originalOutput = Console.Out;

        using var writer = new StringWriter(CultureInfo.InvariantCulture);

        try
        {
            Console.SetOut(writer);

            var exitCode = handler.Execute(["ai-status"]);

            return (exitCode, writer.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }
}
