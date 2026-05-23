using System.Globalization;
using DevMemory.Application.Abstractions.Ai;
using DevMemory.Application.Ai;
using DevMemory.Application.Models.Ai;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands;
using DevMemory.Cli.Tests.TestSupport;

namespace DevMemory.Cli.Tests;

[Collection(CliTestCollections.ConsoleOutput)]
public sealed class SemanticSearchCommandHandlerTests
{
    [Fact]
    public void Execute_WhenQueryIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => handler.Execute(["semantic-search"]));

        // Assert
        Assert.Equal(
            "Usage: devmemory semantic-search <query> [--limit <number>]",
            exception.Message);
    }

    [Fact]
    public void Execute_WhenLimitIsInvalid_ThrowsArgumentException()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => handler.Execute(["semantic-search", "query", "--limit", "0"]));

        // Assert
        Assert.Equal("Option --limit must be a positive integer.", exception.Message);
    }

    [Fact]
    public void Execute_WhenSemanticSearchIsNotConfigured_ReturnsFailureAndPrintsGuidance()
    {
        // Arrange
        var handler = CreateHandler(optionsFactory: () => new AiRuntimeOptions());

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["semantic-search", "estimate", "revision"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("Semantic search is not configured.", result.Error, StringComparison.Ordinal);
        Assert.Contains(
            "Configure both an embedding provider and a vector store before searching.",
            result.Error,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenEmbeddingProviderHasNoAdapter_ReturnsFailureAndPrintsGuidance()
    {
        // Arrange
        var handler = CreateHandler(embeddingServiceFactory: _ => null);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["semantic-search", "estimate", "revision"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains(
            "Embedding provider 'ollama' is configured, but no adapter is available yet.",
            result.Error,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenVectorStoreHasNoAdapter_ReturnsFailureAndPrintsGuidance()
    {
        // Arrange
        var handler = CreateHandler(vectorMemoryStoreFactory: _ => null);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["semantic-search", "estimate", "revision"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains(
            "Vector store 'qdrant' is configured, but no adapter is available yet.",
            result.Error,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenSemanticSearchSucceeds_PrintsResults()
    {
        // Arrange
        var handler = CreateHandler();

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["semantic-search", "estimate", "revision", "--limit", "3"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Contains("DevMemory semantic search", result.Output, StringComparison.Ordinal);
        Assert.Contains("Query: estimate revision", result.Output, StringComparison.Ordinal);
        Assert.Contains("Limit: 3", result.Output, StringComparison.Ordinal);
        Assert.Contains("Embedding provider: ollama", result.Output, StringComparison.Ordinal);
        Assert.Contains("Embedding model: nomic-embed-text", result.Output, StringComparison.Ordinal);
        Assert.Contains("Vector store: qdrant", result.Output, StringComparison.Ordinal);
        Assert.Contains("Results: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("1. Estimate revision cloning", result.Output, StringComparison.Ordinal);
        Assert.Contains("Project: DevMemory", result.Output, StringComparison.Ordinal);
        Assert.Contains("Area: AI", result.Output, StringComparison.Ordinal);
        Assert.Contains("Score: 0.91", result.Output, StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates a semantic search command handler with test defaults.
    /// </summary>
    private static SemanticSearchCommandHandler CreateHandler(
        Func<AiRuntimeOptions>? optionsFactory = null,
        Func<AiRuntimeOptions, IEmbeddingService?>? embeddingServiceFactory = null,
        Func<AiRuntimeOptions, IVectorMemoryStore?>? vectorMemoryStoreFactory = null)
    {
        return new SemanticSearchCommandHandler(
            optionsFactory ?? CreateSemanticSearchOptions,
            embeddingServiceFactory ?? (_ => new FakeEmbeddingService()),
            vectorMemoryStoreFactory ?? (_ => new FakeVectorMemoryStore()),
            static (embeddingService, vectorMemoryStore) =>
                new MemorySemanticSearchService(embeddingService, vectorMemoryStore));
    }

    /// <summary>
    /// Creates semantic search options configured with Ollama embeddings and Qdrant.
    /// </summary>
    private static AiRuntimeOptions CreateSemanticSearchOptions()
    {
        return new AiRuntimeOptions
        {
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
        SemanticSearchCommandHandler handler,
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
                    Project = "DevMemory",
                    Area = "AI",
                    Score = 0.91m
                }
            ];

            return Task.FromResult(results);
        }
    }
}
