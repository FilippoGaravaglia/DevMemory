using DevMemory.Application;
using DevMemory.Application.Abstractions.Memory;
using DevMemory.Application.Models.Ai.Chat;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands.System;
using DevMemory.Core;

namespace DevMemory.Cli.Tests.Commands.System;

public sealed class DoctorCommandHandlerTests
{
    [Fact]
    public void Execute_WhenEverythingIsReady_PrintsReadyResult()
    {
        // Arrange
        var handler = CreateHandler(
            memories: [CreateMemory()],
            optionsFactory: CreateFullRagOptions,
            configFileExists: true,
            markdownDirectoryExists: true,
            gitCheck: CreateGitOkCheck);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["doctor"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("DevMemory doctor", result.Output, StringComparison.Ordinal);
        Assert.Contains("[OK] Storage", result.Output, StringComparison.Ordinal);
        Assert.Contains("[OK] Markdown", result.Output, StringComparison.Ordinal);
        Assert.Contains("[OK] Configuration", result.Output, StringComparison.Ordinal);
        Assert.Contains("[OK] AI runtime", result.Output, StringComparison.Ordinal);
        Assert.Contains("[OK] Git", result.Output, StringComparison.Ordinal);
        Assert.Contains("Result: ready", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenConfigurationIsMissing_ReturnsSuccessWithAttention()
    {
        // Arrange
        var handler = CreateHandler(
            memories: [CreateMemory()],
            optionsFactory: CreateFullRagOptions,
            configFileExists: false,
            markdownDirectoryExists: true,
            gitCheck: CreateGitOkCheck);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["doctor"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("[ATTENTION] Configuration", result.Output, StringComparison.Ordinal);
        Assert.Contains("Result: attention required", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenAiRuntimeIsNotConfigured_ReturnsSuccessWithAttention()
    {
        // Arrange
        var handler = CreateHandler(
            memories: [CreateMemory()],
            optionsFactory: CreateDisabledAiOptions,
            configFileExists: true,
            markdownDirectoryExists: true,
            gitCheck: CreateGitOkCheck);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["doctor"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("[ATTENTION] AI runtime", result.Output, StringComparison.Ordinal);
        Assert.Contains("AI runtime is not configured.", result.Output, StringComparison.Ordinal);
        Assert.Contains("Result: attention required", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenGitIsNotAvailable_ReturnsSuccessWithAttention()
    {
        // Arrange
        var handler = CreateHandler(
            memories: [CreateMemory()],
            optionsFactory: CreateFullRagOptions,
            configFileExists: true,
            markdownDirectoryExists: true,
            gitCheck: CreateGitAttentionCheck);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["doctor"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("[ATTENTION] Git", result.Output, StringComparison.Ordinal);
        Assert.Contains("Git is not available.", result.Output, StringComparison.Ordinal);
        Assert.Contains("Result: attention required", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenStorageFails_ReturnsFailure()
    {
        // Arrange
        var repository = new ThrowingMemoryRepository();
        var exporter = new TestMemoryExporter();
        var memoryService = new MemoryService(repository, exporter);

        var handler = new DoctorCommandHandler(
            memoryService,
            CreateFullRagOptions,
            () => "/tmp/devmemory/config.json",
            _ => true,
            _ => true,
            CreateGitOkCheck);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["doctor"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Empty(result.Error);

        Assert.Contains("[FAIL] Storage", result.Output, StringComparison.Ordinal);
        Assert.Contains("Storage file is corrupted.", result.Output, StringComparison.Ordinal);
        Assert.Contains("Result: failed", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenUnexpectedArgumentsAreProvided_ReturnsInvalidCommand()
    {
        // Arrange
        var handler = CreateHandler(
            memories: [],
            optionsFactory: CreateDisabledAiOptions,
            configFileExists: false,
            markdownDirectoryExists: false,
            gitCheck: CreateGitAttentionCheck);

        // Act
        var result = ExecuteAndCaptureOutput(handler, ["doctor", "--unknown"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("Usage:", result.Error, StringComparison.Ordinal);
        Assert.Contains("devmemory doctor", result.Error, StringComparison.Ordinal);
    }

    #region Helpers

    private static DoctorCommandHandler CreateHandler(
        IReadOnlyCollection<TaskMemory> memories,
        Func<AiRuntimeOptions> optionsFactory,
        bool configFileExists,
        bool markdownDirectoryExists,
        Func<DoctorCommandHandler.DoctorCheck> gitCheck)
    {
        var repository = new TestMemoryRepository(memories);
        var exporter = new TestMemoryExporter();
        var memoryService = new MemoryService(repository, exporter);

        return new DoctorCommandHandler(
            memoryService,
            optionsFactory,
            () => "/tmp/devmemory/config.json",
            _ => configFileExists,
            _ => markdownDirectoryExists,
            gitCheck);
    }

    private static DoctorCommandHandler.DoctorCheck CreateGitOkCheck()
    {
        return DoctorCommandHandler.DoctorCheck.Ok(
            "Git",
            "git version 2.45.0");
    }

    private static DoctorCommandHandler.DoctorCheck CreateGitAttentionCheck()
    {
        return DoctorCommandHandler.DoctorCheck.Attention(
            "Git",
            "Git is not available.");
    }

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

    private static AiRuntimeOptions CreateDisabledAiOptions()
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

    private static TaskMemory CreateMemory()
    {
        return new TaskMemory
        {
            Id = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4"),
            Title = "Doctor command",
            Project = "DevMemory",
            Area = "Diagnostics",
            Branch = "main",
            Tags = ["doctor"],
            Problem = "Need general diagnostics.",
            Solution = "Add doctor command.",
            Decisions = ["Keep general diagnostics separate from AI diagnostics."],
            FilesTouched = ["DoctorCommandHandler.cs"],
            Tests = ["DoctorCommandHandlerTests"],
            LessonsLearned = "General diagnostics improve onboarding.",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static CommandResult ExecuteAndCaptureOutput(
        DoctorCommandHandler handler,
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
            return "/tmp/devmemory/devmemory.json";
        }

        public string GetMarkdownDirectoryPath()
        {
            return "/tmp/devmemory/markdown";
        }
    }

    private sealed class ThrowingMemoryRepository : IMemoryRepository
    {
        public List<TaskMemory> Load()
        {
            throw new InvalidOperationException("Storage file is corrupted.");
        }

        public void Save(List<TaskMemory> memories)
        {
        }

        public string GetStorageFilePath()
        {
            return "/tmp/devmemory/devmemory.json";
        }

        public string GetMarkdownDirectoryPath()
        {
            return "/tmp/devmemory/markdown";
        }
    }

    private sealed class TestMemoryExporter : IMemoryExporter
    {
        public string Export(TaskMemory memory)
        {
            return "/tmp/devmemory/markdown/memory.md";
        }

        public void Delete(TaskMemory memory)
        {
        }
    }

    private sealed record CommandResult(
        int ExitCode,
        string Output,
        string Error);

    #endregion
}
