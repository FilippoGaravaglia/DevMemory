using DevMemory.Application;
using DevMemory.Application.Abstractions;
using DevMemory.Application.Abstractions.Memory;
using DevMemory.Application.Models.Ai.Chat;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands.Ai;
using DevMemory.Core;

namespace DevMemory.Cli.Tests.Commands.Ai;

public sealed class RelatedCommandHandlerTests
{
    [Fact]
    public void Execute_WhenMemoryIdIsMissing_ReturnsInvalidCommand()
    {
        // Arrange
        var handler = CreateHandler([]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["related"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("Memory id is required.", result.Error, StringComparison.Ordinal);
        Assert.Contains("devmemory related <memory-id>", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenMemoryIdIsInvalid_ReturnsInvalidCommand()
    {
        // Arrange
        var handler = CreateHandler([]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["related", "not-a-guid"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("Invalid memory id.", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenMemoryDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");
        var handler = CreateHandler([]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["related", memoryId.ToString("D")]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains($"Memory not found: {memoryId}", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenSemanticSearchIsNotConfigured_ReturnsFailure()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");
        var handler = CreateHandler([CreateMemory(memoryId)]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["related", memoryId.ToString("D")]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("Related memories search is not configured.", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenRelatedMemoriesAreFound_PrintsResultsAndExcludesSourceMemory()
    {
        // Arrange
        var sourceMemoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");
        var relatedMemoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var sourceMemory = CreateMemory(sourceMemoryId);
        var embeddingService = new TestEmbeddingService();
        var vectorStore = new TestVectorMemoryStore(
        [
            new VectorMemorySearchResult
            {
                MemoryId = sourceMemoryId,
                Title = "Source memory",
                Project = "DevMemory",
                Area = "AI",
                Text = "Source memory text.",
                Score = 0.99m
            },
            new VectorMemorySearchResult
            {
                MemoryId = relatedMemoryId,
                Title = "Related memory",
                Project = "DevMemory",
                Area = "RAG",
                Text = "Related memory text.",
                Score = 0.82m
            }
        ]);

        var handler = CreateHandler(
            [sourceMemory],
            CreateQdrantAiRuntimeOptions,
            _ => embeddingService,
            _ => vectorStore);

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["related", sourceMemoryId.ToString("D"), "--limit", "3"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Related memories", result.Output, StringComparison.Ordinal);
        Assert.Contains("Source:", result.Output, StringComparison.Ordinal);
        Assert.Contains(sourceMemoryId.ToString("D"), result.Output, StringComparison.Ordinal);
        Assert.Contains("Related memory", result.Output, StringComparison.Ordinal);
        Assert.Contains(relatedMemoryId.ToString("D"), result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("1. Source memory", result.Output, StringComparison.Ordinal);

        Assert.Single(embeddingService.Requests);
        Assert.Equal(4, vectorStore.LastLimit);
    }

    [Fact]
    public void Execute_WhenShowPreviewIsProvided_PrintsPreview()
    {
        // Arrange
        var sourceMemoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");
        var relatedMemoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var embeddingService = new TestEmbeddingService();
        var vectorStore = new TestVectorMemoryStore(
        [
            new VectorMemorySearchResult
            {
                MemoryId = relatedMemoryId,
                Title = "Related memory",
                Project = "DevMemory",
                Area = "RAG",
                Text = "Preview text for related memory.",
                Score = 0.82m
            }
        ]);

        var handler = CreateHandler(
            [CreateMemory(sourceMemoryId)],
            CreateQdrantAiRuntimeOptions,
            _ => embeddingService,
            _ => vectorStore);

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["related", sourceMemoryId.ToString("D"), "--show-preview"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Contains("Preview:", result.Output, StringComparison.Ordinal);
        Assert.Contains("Preview text for related memory.", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenLimitIsInvalid_ReturnsInvalidCommand()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");
        var handler = CreateHandler([CreateMemory(memoryId)]);

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["related", memoryId.ToString("D"), "--limit", "0"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("Option --limit must be a positive integer.", result.Error, StringComparison.Ordinal);
    }

    private static RelatedCommandHandler CreateHandler(IReadOnlyCollection<TaskMemory> memories)
    {
        return CreateHandler(
            memories,
            CreateDisabledAiRuntimeOptions,
            _ => null,
            _ => null);
    }

    private static RelatedCommandHandler CreateHandler(
        IReadOnlyCollection<TaskMemory> memories,
        Func<AiRuntimeOptions> optionsFactory,
        Func<AiRuntimeOptions, IEmbeddingService?> embeddingServiceFactory,
        Func<AiRuntimeOptions, IVectorMemoryStore?> vectorMemoryStoreFactory)
    {
        var repository = new TestMemoryRepository(memories);
        var exporter = new TestMemoryExporter();
        var memoryService = new MemoryService(repository, exporter);

        return new RelatedCommandHandler(
            memoryService,
            optionsFactory,
            embeddingServiceFactory,
            vectorMemoryStoreFactory,
            static (embeddingService, vectorMemoryStore) =>
                new DevMemory.Application.Ai.Search.MemorySemanticSearchService(
                    embeddingService,
                    vectorMemoryStore));
    }

    private static AiRuntimeOptions CreateDisabledAiRuntimeOptions()
    {
        return new AiRuntimeOptions
        {
            Chat = new ChatProviderOptions
            {
                Provider = AiProviderNames.None
            },
            Embedding = new EmbeddingProviderOptions
            {
                Provider = AiProviderNames.None
            },
            VectorStore = new VectorStoreOptions
            {
                Provider = VectorStoreNames.None
            }
        };
    }

    private static AiRuntimeOptions CreateQdrantAiRuntimeOptions()
    {
        return new AiRuntimeOptions
        {
            Chat = new ChatProviderOptions
            {
                Provider = AiProviderNames.None
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

    private static TaskMemory CreateMemory(Guid memoryId)
    {
        return new TaskMemory
        {
            Id = memoryId,
            Title = "Local AI runtime validation",
            Project = "DevMemory",
            Area = "AI",
            Branch = "main",
            Tags = ["ollama", "qdrant", "rag"],
            Problem = "Validate the local AI runtime.",
            Solution = "Indexed memories and asked RAG questions.",
            Decisions = ["Use Ollama locally", "Use Qdrant as derived vector index"],
            FilesTouched = ["README.md"],
            Tests = ["devmemory semantic-search"],
            LessonsLearned = "Local RAG works when services are configured.",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static CommandResult ExecuteAndCaptureOutput(
        RelatedCommandHandler handler,
        string[] args)
    {
        var originalOutput = Console.Out;
        var originalError = Console.Error;

        using var outputWriter = new StringWriter();
        using var errorWriter = new StringWriter();

        try
        {
            Console.SetOut(outputWriter);
            Console.SetError(errorWriter);

            var exitCode = handler.Execute(args);

            return new CommandResult(
                exitCode,
                outputWriter.ToString(),
                errorWriter.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
            Console.SetError(originalError);
        }
    }

    private sealed class TestMemoryRepository : IMemoryRepository
    {
        private readonly List<TaskMemory> _memories;

        public TestMemoryRepository(IReadOnlyCollection<TaskMemory> memories)
        {
            _memories = memories.ToList();
        }

        public List<TaskMemory> Load()
        {
            return _memories.ToList();
        }

        public void Save(List<TaskMemory> memories)
        {
            _memories.Clear();
            _memories.AddRange(memories);
        }

        public string GetStorageFilePath()
        {
            return "/tmp/devmemory.json";
        }

        public string GetMarkdownDirectoryPath()
        {
            return "/tmp/markdown";
        }
    }

    private sealed class TestMemoryExporter : IMemoryExporter
    {
        public string Export(TaskMemory memory)
        {
            return $"/tmp/markdown/{memory.Id:D}.md";
        }

        public void Delete(TaskMemory memory)
        {
        }
    }

    private sealed class TestEmbeddingService : IEmbeddingService
    {
        public List<EmbeddingRequest> Requests { get; } = [];

        public Task<EmbeddingResponse> GenerateEmbeddingAsync(
            EmbeddingRequest request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);

            return Task.FromResult(new EmbeddingResponse
            {
                Vector = [0.1f, 0.2f, 0.3f]
            });
        }
    }

    private sealed class TestVectorMemoryStore : IVectorMemoryStore
    {
        private readonly IReadOnlyCollection<VectorMemorySearchResult> _results;

        public TestVectorMemoryStore(IReadOnlyCollection<VectorMemorySearchResult> results)
        {
            _results = results;
        }

        public int LastLimit { get; private set; }

        public Task UpsertAsync(
            VectorMemoryDocument document,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<VectorMemorySearchResult>> SearchAsync(
            IReadOnlyList<float> queryVector,
            int limit,
            CancellationToken cancellationToken)
        {
            LastLimit = limit;

            return Task.FromResult(_results);
        }

        public Task DeleteAsync(
            Guid memoryId,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed record CommandResult(
        int ExitCode,
        string Output,
        string Error);
}
