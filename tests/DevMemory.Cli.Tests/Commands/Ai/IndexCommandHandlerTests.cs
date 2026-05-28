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

    [Fact]
    public void Execute_WhenDryRunIsProvided_PrintsIndexPreviewWithoutResolvingAiAdapters()
    {
        // Arrange
        var memoryId = Guid.Parse("741bf4b6-2b81-48a5-beae-1d0208e521d2");

        var handler = CreateDryRunHandler(
        [
            new VectorMemoryDocument
            {
                MemoryId = memoryId,
                Title = "Estimate revision cloning",
                Project = "LogicalCommon",
                Area = "Estimate",
                Branch = "feature/revisions",
                Tags = ["dotnet", "mongodb"],
                FilesTouched = ["src/EstimateService.cs", "tests/EstimateServiceTests.cs"],
                Text = "Revision cloning was handled by normalizing null collections."
            }
        ]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["index", "--dry-run"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory vector index dry-run", result.Output, StringComparison.Ordinal);
        Assert.Contains("No embeddings will be generated.", result.Output, StringComparison.Ordinal);
        Assert.Contains("No vector store writes will be performed.", result.Output, StringComparison.Ordinal);
        Assert.Contains("Total documents: 1", result.Output, StringComparison.Ordinal);

        Assert.Contains("Estimate revision cloning", result.Output, StringComparison.Ordinal);
        Assert.Contains(memoryId.ToString("D"), result.Output, StringComparison.Ordinal);
        Assert.Contains("Project: LogicalCommon", result.Output, StringComparison.Ordinal);
        Assert.Contains("Area: Estimate", result.Output, StringComparison.Ordinal);
        Assert.Contains("Branch: feature/revisions", result.Output, StringComparison.Ordinal);
        Assert.Contains("Tags: dotnet, mongodb", result.Output, StringComparison.Ordinal);
        Assert.Contains("Files touched: 2", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenDryRunHasNoDocuments_PrintsEmptyIndexMessage()
    {
        // Arrange
        var handler = CreateDryRunHandler([]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["index", "--dry-run"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory vector index dry-run", result.Output, StringComparison.Ordinal);
        Assert.Contains("Total documents: 0", result.Output, StringComparison.Ordinal);
        Assert.Contains("No memories available for indexing.", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenUnknownOptionIsProvided_ThrowsArgumentException()
    {
        // Arrange
        var handler = CreateDryRunHandler([]);

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => handler.Execute(["index", "--unknown"]));

        // Assert
        Assert.Equal("Usage: devmemory index [--dry-run] [--force] [--limit <number>] [--project <project>] [--area <area>] [--tag <tag>]", exception.Message);
    }

    [Fact]
    public void Execute_WhenDryRunAndForceAreProvided_ThrowsArgumentException()
    {
        // Arrange
        var handler = CreateDryRunHandler([]);

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => handler.Execute(["index", "--dry-run", "--force"]));

        // Assert
        Assert.Equal("Options --dry-run and --force cannot be used together.", exception.Message);
    }

    [Fact]
    public void Execute_WhenForceIsProvided_PrintsForceIndexingEnabled()
    {
        // Arrange
        var documents = new[]
        {
            new VectorMemoryDocument
            {
                MemoryId = Guid.Parse("741bf4b6-2b81-48a5-beae-1d0208e521d2"),
                DocumentId = "741bf4b6-2b81-48a5-beae-1d0208e521d2",
                ContentHash = "hash-1",
                Title = "Force indexing memory",
                Project = "DevMemory",
                Area = "AI",
                Text = "Force indexing should regenerate this embedding."
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
        var result = ExecuteAndCaptureOutput(handler, ["index", "--force"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Contains("Force indexing: yes", result.Output, StringComparison.Ordinal);
        Assert.Contains("Indexed documents: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("Skipped documents: 0", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenLimitIsInvalid_ThrowsArgumentException()
    {
        // Arrange
        var handler = CreateDryRunHandler([]);

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => handler.Execute(["index", "--limit", "0"]));

        // Assert
        Assert.Equal("Option --limit must be a positive integer.", exception.Message);
    }

    [Fact]
    public void Execute_WhenDryRunLimitIsProvided_PrintsOnlyLimitedDocuments()
    {
        // Arrange
        var firstMemoryId = Guid.Parse("741bf4b6-2b81-48a5-beae-1d0208e521d2");
        var secondMemoryId = Guid.Parse("39478526-4706-455e-9444-e18d01771240");

        var handler = CreateDryRunHandler(
        [
            new VectorMemoryDocument
            {
                MemoryId = firstMemoryId,
                DocumentId = firstMemoryId.ToString("D"),
                ContentHash = "hash-1",
                Title = "First memory",
                Text = "First memory text."
            },
            new VectorMemoryDocument
            {
                MemoryId = secondMemoryId,
                DocumentId = secondMemoryId.ToString("D"),
                ContentHash = "hash-2",
                Title = "Second memory",
                Text = "Second memory text."
            }
        ]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["index", "--dry-run", "--limit", "1"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Limit: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("Total documents: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("First memory", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Second memory", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenLimitIsProvided_IndexesOnlyLimitedDocuments()
    {
        // Arrange
        var firstMemoryId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var secondMemoryId = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff");

        var documents = new[]
        {
            new VectorMemoryDocument
            {
                MemoryId = firstMemoryId,
                DocumentId = firstMemoryId.ToString("D"),
                ContentHash = "hash-1",
                Title = "First memory",
                Text = "First memory text."
            },
            new VectorMemoryDocument
            {
                MemoryId = secondMemoryId,
                DocumentId = secondMemoryId.ToString("D"),
                ContentHash = "hash-2",
                Title = "Second memory",
                Text = "Second memory text."
            }
        };

        var vectorStore = new FakeVectorMemoryStore();

        var handler = new IndexCommandHandler(
            CreateSemanticSearchOptions,
            _ => new FakeEmbeddingService(),
            _ => vectorStore,
            () => documents,
            static (embeddingService, vectorMemoryStore) =>
                new MemoryVectorIndexingService(embeddingService, vectorMemoryStore));

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["index", "--limit", "1"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Limit: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("Total documents: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("Indexed documents: 1", result.Output, StringComparison.Ordinal);

        var indexedDocument = Assert.Single(vectorStore.Documents);
        Assert.Equal(firstMemoryId, indexedDocument.MemoryId);
    }

    [Fact]
    public void Execute_WhenAllDocumentsAreSkipped_PrintsNoChangesGuidance()
    {
        // Arrange
        var memoryId = Guid.Parse("741bf4b6-2b81-48a5-beae-1d0208e521d2");

        var documents = new[]
        {
            new VectorMemoryDocument
            {
                MemoryId = memoryId,
                DocumentId = memoryId.ToString("D"),
                ContentHash = "hash-1",
                Title = "Already indexed memory",
                Project = "DevMemory",
                Area = "AI",
                Text = "This memory has already been indexed."
            }
        };

        var vectorStore = new FakeIncrementalVectorMemoryStore(indexedContentHash: "hash-1");

        var handler = new IndexCommandHandler(
            CreateSemanticSearchOptions,
            _ => new FakeEmbeddingService(),
            _ => vectorStore,
            () => documents,
            static (embeddingService, vectorMemoryStore) =>
                new MemoryVectorIndexingService(embeddingService, vectorMemoryStore));

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["index"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Total documents: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("Indexed documents: 0", result.Output, StringComparison.Ordinal);
        Assert.Contains("Skipped documents: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("Failed documents: 0", result.Output, StringComparison.Ordinal);

        Assert.Contains("No changes detected. All documents were already indexed.", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory index --force", result.Output, StringComparison.Ordinal);

        Assert.Empty(vectorStore.Documents);
    }

    [Fact]
    public void Execute_WhenNoDocumentsAreAvailable_PrintsCreateMemoryGuidance()
    {
        // Arrange
        var handler = new IndexCommandHandler(
            CreateSemanticSearchOptions,
            _ => new FakeEmbeddingService(),
            _ => new FakeVectorMemoryStore(),
            () => [],
            static (embeddingService, vectorMemoryStore) =>
                new MemoryVectorIndexingService(embeddingService, vectorMemoryStore));

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["index"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Total documents: 0", result.Output, StringComparison.Ordinal);
        Assert.Contains("Indexed documents: 0", result.Output, StringComparison.Ordinal);
        Assert.Contains("Skipped documents: 0", result.Output, StringComparison.Ordinal);
        Assert.Contains("Failed documents: 0", result.Output, StringComparison.Ordinal);

        Assert.Contains("No memories are available for indexing.", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory add", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenProjectOptionHasNoValue_ThrowsArgumentException()
    {
        // Arrange
        var handler = CreateDryRunHandler([]);

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => handler.Execute(["index", "--project"]));

        // Assert
        Assert.Equal("Option --project requires a value.", exception.Message);
    }

    [Fact]
    public void Execute_WhenDryRunFiltersAreProvided_PrintsOnlyMatchingDocuments()
    {
        // Arrange
        var matchingMemoryId = Guid.Parse("741bf4b6-2b81-48a5-beae-1d0208e521d2");
        var otherMemoryId = Guid.Parse("39478526-4706-455e-9444-e18d01771240");

        var handler = CreateDryRunHandler(
        [
            new VectorMemoryDocument
            {
                MemoryId = matchingMemoryId,
                DocumentId = matchingMemoryId.ToString("D"),
                ContentHash = "hash-1",
                Title = "Matching memory",
                Project = "LogicalCommon",
                Area = "Estimate",
                Tags = ["mongodb", "dotnet"],
                Text = "Matching memory text."
            },
            new VectorMemoryDocument
            {
                MemoryId = otherMemoryId,
                DocumentId = otherMemoryId.ToString("D"),
                ContentHash = "hash-2",
                Title = "Other memory",
                Project = "DevMemory",
                Area = "AI",
                Tags = ["rag"],
                Text = "Other memory text."
            }
        ]);

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["index", "--dry-run", "--project", "logicalcommon", "--area", "estimate", "--tag", "MongoDB"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Project filter: logicalcommon", result.Output, StringComparison.Ordinal);
        Assert.Contains("Area filter: estimate", result.Output, StringComparison.Ordinal);
        Assert.Contains("Tag filter: MongoDB", result.Output, StringComparison.Ordinal);
        Assert.Contains("Total documents: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("Matching memory", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Other memory", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenProjectFilterIsProvided_IndexesOnlyMatchingDocuments()
    {
        // Arrange
        var matchingMemoryId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var otherMemoryId = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff");

        var documents = new[]
        {
            new VectorMemoryDocument
            {
                MemoryId = matchingMemoryId,
                DocumentId = matchingMemoryId.ToString("D"),
                ContentHash = "hash-1",
                Title = "LogicalCommon memory",
                Project = "LogicalCommon",
                Area = "Estimate",
                Tags = ["mongodb"],
                Text = "LogicalCommon memory text."
            },
            new VectorMemoryDocument
            {
                MemoryId = otherMemoryId,
                DocumentId = otherMemoryId.ToString("D"),
                ContentHash = "hash-2",
                Title = "DevMemory memory",
                Project = "DevMemory",
                Area = "AI",
                Tags = ["rag"],
                Text = "DevMemory memory text."
            }
        };

        var vectorStore = new FakeVectorMemoryStore();

        var handler = new IndexCommandHandler(
            CreateSemanticSearchOptions,
            _ => new FakeEmbeddingService(),
            _ => vectorStore,
            () => documents,
            static (embeddingService, vectorMemoryStore) =>
                new MemoryVectorIndexingService(embeddingService, vectorMemoryStore));

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["index", "--project", "logicalcommon"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Project filter: logicalcommon", result.Output, StringComparison.Ordinal);
        Assert.Contains("Total documents: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("Indexed documents: 1", result.Output, StringComparison.Ordinal);

        var indexedDocument = Assert.Single(vectorStore.Documents);
        Assert.Equal(matchingMemoryId, indexedDocument.MemoryId);
    }

    #region Helpers

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
    /// Creates an index command handler configured for dry-run tests.
    /// </summary>
    private static IndexCommandHandler CreateDryRunHandler(
        IReadOnlyCollection<VectorMemoryDocument> documents)
    {
        return new IndexCommandHandler(
            static () => throw new InvalidOperationException("Options should not be resolved during dry-run."),
            static _ => throw new InvalidOperationException("Embedding service should not be resolved during dry-run."),
            static _ => throw new InvalidOperationException("Vector store should not be resolved during dry-run."),
            () => documents,
            static (_, _) => throw new InvalidOperationException("Indexing service should not be created during dry-run."));
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

    private sealed class FakeIncrementalVectorMemoryStore : IVectorMemoryStore, IVectorMemoryIndexStateStore
    {
        private readonly string? _indexedContentHash;

        public FakeIncrementalVectorMemoryStore(string? indexedContentHash)
        {
            _indexedContentHash = indexedContentHash;
        }

        public List<VectorMemoryDocument> Documents { get; } = [];

        public Task<string?> TryGetIndexedContentHashAsync(
            string documentId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_indexedContentHash);
        }

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

    #endregion
}
