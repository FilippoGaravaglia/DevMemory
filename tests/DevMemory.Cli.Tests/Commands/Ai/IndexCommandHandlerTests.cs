using System.Globalization;
using DevMemory.Application.Abstractions;
using DevMemory.Application.Ai.Indexing;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands.Ai;
using DevMemory.Cli.Tests.TestSupport;

namespace DevMemory.Cli.Tests.Commands.Ai;

[Collection(CliTestCollections.ConsoleOutput)]
public sealed class IndexCommandHandlerTests
{
    [Fact]
    public void Execute_WhenSemanticSearchIsNotConfigured_ReturnsFailureAndPrintsGuidance()
    {
        // Arrange
        var handler = new IndexCommandHandler(
            () => new AiRuntimeOptions(),
            _ => new FakeEmbeddingService(),
            _ => new FakeVectorMemoryStore(),
            () => [],
            static (embeddingService, vectorMemoryStore) =>
                new MemoryVectorIndexingService(embeddingService, vectorMemoryStore));

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["index"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("Semantic search is not configured.", result.Error, StringComparison.Ordinal);
        Assert.Contains(
            "Configure both an embedding provider and a vector store before indexing.",
            result.Error,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenEmbeddingProviderHasNoAdapter_ReturnsFailureAndPrintsGuidance()
    {
        // Arrange
        var handler = new IndexCommandHandler(
            CreateSemanticSearchOptions,
            _ => null,
            _ => new FakeVectorMemoryStore(),
            () => [],
            static (embeddingService, vectorMemoryStore) =>
                new MemoryVectorIndexingService(embeddingService, vectorMemoryStore));

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["index"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains(
            "Embedding provider 'ollama' is configured, but no adapter is available yet.",
            result.Error,
            StringComparison.Ordinal);
        Assert.Contains("Currently implemented embedding providers: ollama", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenVectorStoreHasNoAdapter_ReturnsFailureAndPrintsGuidance()
    {
        // Arrange
        var handler = new IndexCommandHandler(
            CreateSemanticSearchOptions,
            _ => new FakeEmbeddingService(),
            _ => null,
            () => [],
            static (embeddingService, vectorMemoryStore) =>
                new MemoryVectorIndexingService(embeddingService, vectorMemoryStore));

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["index"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains(
            "Vector store 'qdrant' is configured, but no adapter is available yet.",
            result.Error,
            StringComparison.Ordinal);
        Assert.Contains("Currently implemented vector stores: qdrant", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenDocumentsAreIndexed_ReturnsSuccessAndPrintsResult()
    {
        // Arrange
        var documents = new[]
        {
            new VectorMemoryDocument
            {
                MemoryId = Guid.NewGuid(),
                Title = "Estimate revision cloning",
                Project = "DevMemory",
                Area = "AI",
                Text = "Implemented estimate revision cloning."
            }
        };

        var handler = new IndexCommandHandler(
            CreateSemanticSearchOptions,
            _ => new FakeEmbeddingService(),
            _ => new FakeVectorMemoryStore(),
            () => documents,
            static (embeddingService, vectorMemoryStore) =>
                new MemoryVectorIndexingService(embeddingService, vectorMemoryStore));

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["index"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Contains("DevMemory vector index", result.Output, StringComparison.Ordinal);
        Assert.Contains("Embedding provider: ollama", result.Output, StringComparison.Ordinal);
        Assert.Contains("Embedding model: nomic-embed-text", result.Output, StringComparison.Ordinal);
        Assert.Contains("Vector store: qdrant", result.Output, StringComparison.Ordinal);
        Assert.Contains("Total documents: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("Indexed documents: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("Failed documents: 0", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenIndexingHasFailures_ReturnsFailureAndPrintsErrors()
    {
        // Arrange
        var documents = new[]
        {
            new VectorMemoryDocument
            {
                MemoryId = Guid.Empty,
                Text = "Invalid document."
            }
        };

        var handler = new IndexCommandHandler(
            CreateSemanticSearchOptions,
            _ => new FakeEmbeddingService(),
            _ => new FakeVectorMemoryStore(),
            () => documents,
            static (embeddingService, vectorMemoryStore) =>
                new MemoryVectorIndexingService(embeddingService, vectorMemoryStore));

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["index"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Contains("Total documents: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("Indexed documents: 0", result.Output, StringComparison.Ordinal);
        Assert.Contains("Failed documents: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("Errors:", result.Output, StringComparison.Ordinal);
        Assert.Contains("Vector document memory id cannot be empty.", result.Output, StringComparison.Ordinal);
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
        IndexCommandHandler handler,
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
        public List<VectorMemoryDocument> Documents { get; } = [];

        public Task UpsertAsync(
            VectorMemoryDocument document,
            CancellationToken cancellationToken)
        {
            Documents.Add(document);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<VectorMemorySearchResult>> SearchAsync(
            IReadOnlyList<float> queryVector,
            int limit,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
