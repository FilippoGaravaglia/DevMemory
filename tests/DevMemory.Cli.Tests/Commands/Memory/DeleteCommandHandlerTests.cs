using DevMemory.Application;
using DevMemory.Application.Abstractions;
using DevMemory.Application.Abstractions.Memory;
using DevMemory.Application.Models.Ai.Chat;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands.Memory;
using DevMemory.Core;

namespace DevMemory.Cli.Tests.Commands.Memory;

public sealed class DeleteCommandHandlerTests
{
    [Fact]
    public void Execute_WhenMemoryIdIsMissing_ReturnsInvalidCommand()
    {
        // Arrange
        var handler = CreateHandler([]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["delete"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("Usage:", result.Error, StringComparison.Ordinal);
        Assert.Contains("devmemory delete <memory-id> [--yes]", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenMemoryIdIsInvalid_ReturnsInvalidCommand()
    {
        // Arrange
        var handler = CreateHandler([]);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["delete", "not-a-guid"]);

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
        var result = ExecuteAndCaptureOutput(handler, ["delete", memoryId.ToString("D"), "--yes"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains($"Memory not found: {memoryId}", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenMemoryExistsAndYesIsProvided_DeletesMemory()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");

        var memory = CreateMemory(memoryId);
        var repository = new TestMemoryRepository([memory]);
        var handler = CreateHandler(repository);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["delete", memoryId.ToString("D"), "--yes"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Memory deleted successfully.", result.Output, StringComparison.Ordinal);
        Assert.Contains(memoryId.ToString("D"), result.Output, StringComparison.Ordinal);
        Assert.Contains("Release v0.1.3 finalized", result.Output, StringComparison.Ordinal);
        Assert.Contains("Vector index: skipped", result.Output, StringComparison.Ordinal);

        Assert.Empty(repository.Load());
    }

    [Fact]
    public void Execute_WhenMemoryExistsButConfirmationIsRejected_DoesNotDeleteMemory()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");

        var memory = CreateMemory(memoryId);
        var repository = new TestMemoryRepository([memory]);
        var handler = CreateHandler(repository);

        // Act
        var result = ExecuteAndCaptureOutput(
            handler,
            ["delete", memoryId.ToString("D")],
            input: "no");

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Delete cancelled.", result.Output, StringComparison.Ordinal);

        Assert.Single(repository.Load());
    }

    [Fact]
    public void Execute_WhenVectorStoreIsConfigured_DeletesVectorPoint()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");

        var memory = CreateMemory(memoryId);
        var repository = new TestMemoryRepository([memory]);
        var exporter = new TestMemoryExporter();
        var service = new MemoryService(repository, exporter);
        var vectorStore = new TestVectorMemoryStore();

        var handler = new DeleteCommandHandler(
            service,
            CreateQdrantAiRuntimeOptions,
            _ => vectorStore);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["delete", memoryId.ToString("D"), "--yes"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Memory deleted successfully.", result.Output, StringComparison.Ordinal);
        Assert.Contains("Vector index: deleted", result.Output, StringComparison.Ordinal);
        Assert.Contains(memoryId, vectorStore.DeletedMemoryIds);

        Assert.Empty(repository.Load());
    }

    [Fact]
    public void Execute_WhenVectorCleanupFails_StillReturnsSuccessAndPrintsWarning()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");

        var memory = CreateMemory(memoryId);
        var repository = new TestMemoryRepository([memory]);
        var exporter = new TestMemoryExporter();
        var service = new MemoryService(repository, exporter);
        var vectorStore = new TestVectorMemoryStore
        {
            DeleteException = new InvalidOperationException("Qdrant is not reachable.")
        };

        var handler = new DeleteCommandHandler(
            service,
            CreateQdrantAiRuntimeOptions,
            _ => vectorStore);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["delete", memoryId.ToString("D"), "--yes"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("Memory deleted successfully.", result.Output, StringComparison.Ordinal);
        Assert.Contains("Vector index: cleanup failed", result.Output, StringComparison.Ordinal);
        Assert.Contains("Qdrant is not reachable.", result.Output, StringComparison.Ordinal);
        Assert.Contains("devmemory index --force", result.Output, StringComparison.Ordinal);

        Assert.Empty(repository.Load());
    }

    #region Helpers

    private static DeleteCommandHandler CreateHandler(IReadOnlyCollection<TaskMemory> memories)
    {
        return CreateHandler(new TestMemoryRepository(memories));
    }

    private static DeleteCommandHandler CreateHandler(TestMemoryRepository repository)
    {
        var exporter = new TestMemoryExporter();
        var service = new MemoryService(repository, exporter);

        return new DeleteCommandHandler(
            service,
            CreateDisabledAiRuntimeOptions,
            _ => null);
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
            Title = "Release v0.1.3 finalized",
            Project = "DevMemory",
            Area = "Release",
            Branch = "main",
            Tags = ["release", "github"],
            Problem = "Finalize release.",
            Solution = "Published GitHub release.",
            LessonsLearned = "Keep release assets aligned.",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static CommandResult ExecuteAndCaptureOutput(
        DeleteCommandHandler handler,
        string[] args,
        string? input = null)
    {
        var originalOutput = Console.Out;
        var originalError = Console.Error;
        var originalInput = Console.In;

        using var outputWriter = new StringWriter();
        using var errorWriter = new StringWriter();
        using var inputReader = new StringReader(input ?? string.Empty);

        try
        {
            Console.SetOut(outputWriter);
            Console.SetError(errorWriter);
            Console.SetIn(inputReader);

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
            Console.SetIn(originalInput);
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

    private sealed class TestVectorMemoryStore : IVectorMemoryStore
    {
        public List<Guid> DeletedMemoryIds { get; } = [];

        public Exception? DeleteException { get; init; }

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
            return Task.FromResult<IReadOnlyCollection<VectorMemorySearchResult>>([]);
        }

        public Task DeleteAsync(
            Guid memoryId,
            CancellationToken cancellationToken)
        {
            if (DeleteException is not null)
            {
                throw DeleteException;
            }

            DeletedMemoryIds.Add(memoryId);

            return Task.CompletedTask;
        }
    }

    private sealed record CommandResult(
        int ExitCode,
        string Output,
        string Error);

    #endregion
}
