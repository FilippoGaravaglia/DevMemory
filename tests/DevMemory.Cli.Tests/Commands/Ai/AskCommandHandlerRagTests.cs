using System.Globalization;
using DevMemory.Application.Abstractions;
using DevMemory.Application.Ai.Rag;
using DevMemory.Application.Ai.Search;
using DevMemory.Application.Models.Ai.Chat;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands.Ai;
using DevMemory.Cli.Tests.TestSupport;

namespace DevMemory.Cli.Tests.Commands.Ai;

[Collection(CliTestCollections.ConsoleOutput)]
public sealed class AskCommandHandlerRagTests
{
    [Fact]
    public void Execute_WhenRagIsNotConfigured_ReturnsFailureAndPrintsGuidance()
    {
        // Arrange
        var handler = CreateHandler(optionsFactory: CreateChatOnlyOptions);

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["ask", "--rag", "How did we handle revisions?"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("RAG is not configured.", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenEmbeddingAdapterIsMissing_ReturnsFailureAndPrintsGuidance()
    {
        // Arrange
        var handler = CreateHandler(embeddingServiceFactory: _ => null);

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["ask", "--rag", "How did we handle revisions?"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains(
            "Embedding provider 'ollama' is configured, but no adapter is available yet.",
            result.Error,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenVectorStoreAdapterIsMissing_ReturnsFailureAndPrintsGuidance()
    {
        // Arrange
        var handler = CreateHandler(vectorMemoryStoreFactory: _ => null);

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["ask", "--rag", "How did we handle revisions?"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains(
            "Vector store 'qdrant' is configured, but no adapter is available yet.",
            result.Error,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenRagIsConfigured_PrintsRagAnswer()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["ask", "--rag", "How", "did", "we", "handle", "revisions?", "--limit", "3"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Contains("DevMemory RAG answer", result.Output, StringComparison.Ordinal);
        Assert.Contains("Provider: ollama", result.Output, StringComparison.Ordinal);
        Assert.Contains("Model: llama3.2", result.Output, StringComparison.Ordinal);
        Assert.Contains("Question: How did we handle revisions?", result.Output, StringComparison.Ordinal);
        Assert.Contains("Context items: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("Answer:", result.Output, StringComparison.Ordinal);
        Assert.Contains("Use the revision cloning memory.", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Context:", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenRagLimitIsInvalid_ThrowsArgumentException()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => handler.Execute(["ask", "--rag", "question", "--limit", "0"]));

        // Assert
        Assert.Equal("Option --limit must be a positive integer.", exception.Message);
    }

    [Fact]
    public void Execute_WhenShowContextIsProvided_PrintsRagAnswerWithContext()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["ask", "--rag", "--show-context", "How", "did", "we", "handle", "revisions?"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory RAG answer", result.Output, StringComparison.Ordinal);
        Assert.Contains("Provider: ollama", result.Output, StringComparison.Ordinal);
        Assert.Contains("Model: llama3.2", result.Output, StringComparison.Ordinal);
        Assert.Contains("Question: How did we handle revisions?", result.Output, StringComparison.Ordinal);
        Assert.Contains("Context items: 1", result.Output, StringComparison.Ordinal);

        Assert.Contains("Answer:", result.Output, StringComparison.Ordinal);
        Assert.Contains("Use the revision cloning memory.", result.Output, StringComparison.Ordinal);

        Assert.Contains("Context:", result.Output, StringComparison.Ordinal);
        Assert.Contains("1. Estimate revision cloning", result.Output, StringComparison.Ordinal);
        Assert.Contains("Project: LogicalCommon", result.Output, StringComparison.Ordinal);
        Assert.Contains("Area: Estimate", result.Output, StringComparison.Ordinal);
        Assert.Contains("Score: 0.91", result.Output, StringComparison.Ordinal);
        Assert.Contains(
            "Preview: Revision cloning was handled by normalizing null collections.",
            result.Output,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenRagHasNoContext_PrintsNoIndexedMemoriesMessage()
    {
        // Arrange
        var chatService = new FakeChatCompletionService();

        var handler = CreateHandler(
            chatServiceFactory: _ => chatService,
            vectorMemoryStoreFactory: _ => new EmptyFakeVectorMemoryStore());

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["ask", "--rag", "How", "did", "we", "handle", "revisions?"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory RAG answer", result.Output, StringComparison.Ordinal);
        Assert.Contains("Provider: none", result.Output, StringComparison.Ordinal);
        Assert.Contains("Model: llama3.2", result.Output, StringComparison.Ordinal);
        Assert.Contains("Question: How did we handle revisions?", result.Output, StringComparison.Ordinal);
        Assert.Contains("Context items: 0", result.Output, StringComparison.Ordinal);
        Assert.Contains("No indexed memories were found for this question.", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory index", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory index --dry-run", result.Output, StringComparison.Ordinal);

        Assert.False(chatService.WasCalled);
    }

    #region helpers

    /// <summary>
    /// Creates an ask command handler configured for RAG tests.
    /// </summary>
    private static AskCommandHandler CreateHandler(
        Func<AiRuntimeOptions>? optionsFactory = null,
        Func<AiRuntimeOptions, IChatCompletionService?>? chatServiceFactory = null,
        Func<AiRuntimeOptions, IEmbeddingService?>? embeddingServiceFactory = null,
        Func<AiRuntimeOptions, IVectorMemoryStore?>? vectorMemoryStoreFactory = null)
    {
        return new AskCommandHandler(
            optionsFactory ?? CreateFullRagOptions,
            chatServiceFactory ?? (_ => new FakeChatCompletionService()),
            embeddingServiceFactory ?? (_ => new FakeEmbeddingService()),
            vectorMemoryStoreFactory ?? (_ => new FakeVectorMemoryStore()),
            static (embeddingService, vectorMemoryStore, chatService) =>
                new MemoryRagAnswerService(
                    new MemorySemanticSearchService(embeddingService, vectorMemoryStore),
                    chatService));
    }

    /// <summary>
    /// Creates options with chat only enabled.
    /// </summary>
    private static AiRuntimeOptions CreateChatOnlyOptions()
    {
        return new AiRuntimeOptions
        {
            Chat = new ChatProviderOptions
            {
                Provider = AiProviderNames.Ollama,
                OllamaEndpoint = "http://localhost:11434",
                OllamaChatModel = "llama3.2"
            }
        };
    }

    /// <summary>
    /// Creates options with full local RAG enabled.
    /// </summary>
    private static AiRuntimeOptions CreateFullRagOptions()
    {
        return new AiRuntimeOptions
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
        };
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

    private sealed class FakeEmbeddingService : IEmbeddingService
    {
        public Task<EmbeddingResponse> GenerateEmbeddingAsync(
            EmbeddingRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new EmbeddingResponse
            {
                Provider = AiProviderNames.Ollama,
                Model = request.Model,
                Vector = [0.1f, 0.2f, 0.3f]
            });
        }
    }

    private sealed class FakeVectorMemoryStore : IVectorMemoryStore
    {
        public Task UpsertAsync(
            VectorMemoryDocument document,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyCollection<VectorMemorySearchResult>> SearchAsync(
            IReadOnlyList<float> queryVector,
            int limit,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<VectorMemorySearchResult> results =
            [
                new VectorMemorySearchResult
                {
                    MemoryId = Guid.NewGuid(),
                    Title = "Estimate revision cloning",
                    Project = "LogicalCommon",
                    Area = "Estimate",
                    Text = "Revision cloning was handled by normalizing null collections.",
                    Score = 0.91m
                }
            ];

            return Task.FromResult(results);
        }
    }

    private sealed class FakeChatCompletionService : IChatCompletionService
    {
        public bool WasCalled { get; private set; }

        public Task<ChatCompletionResponse> CompleteAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken)
        {
            WasCalled = true;

            return Task.FromResult(new ChatCompletionResponse
            {
                Provider = AiProviderNames.Ollama,
                Model = request.Model,
                Content = "Use the revision cloning memory."
            });
        }
    }

    private sealed class EmptyFakeVectorMemoryStore : IVectorMemoryStore
    {
        public Task UpsertAsync(
            VectorMemoryDocument document,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyCollection<VectorMemorySearchResult>> SearchAsync(
            IReadOnlyList<float> queryVector,
            int limit,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<VectorMemorySearchResult> results = [];

            return Task.FromResult(results);
        }
    }

    #endregion
}
