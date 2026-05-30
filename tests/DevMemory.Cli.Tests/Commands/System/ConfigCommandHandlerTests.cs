using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands.System;
using DevMemory.Infrastructure;

namespace DevMemory.Cli.Tests.Commands.System;

public sealed class ConfigCommandHandlerTests
{
    [Fact]
    public void Execute_WhenShowIsProvided_PrintsConfigurationAndEffectiveRuntime()
    {
        // Arrange
        ClearEnvironmentVariables();

        var configFilePath = CreateTemporaryConfigFilePath();
        var handler = CreateHandler(configFilePath);

        try
        {
            // Act
            var result = ExecuteAndCaptureOutput(handler, ["config", "show"]);

            // Assert
            Assert.Equal(CliExitCodes.Success, result.ExitCode);
            Assert.Empty(result.Error);

            Assert.Contains("DevMemory configuration", result.Output, StringComparison.Ordinal);
            Assert.Contains($"File: {configFilePath}", result.Output, StringComparison.Ordinal);
            Assert.Contains("chat-provider: -", result.Output, StringComparison.Ordinal);
            Assert.Contains("embedding-provider: -", result.Output, StringComparison.Ordinal);
            Assert.Contains("vector-store: -", result.Output, StringComparison.Ordinal);

            Assert.Contains("Effective AI runtime:", result.Output, StringComparison.Ordinal);
            Assert.Contains("Chat provider: none", result.Output, StringComparison.Ordinal);
            Assert.Contains("Embedding provider: none", result.Output, StringComparison.Ordinal);
            Assert.Contains("Vector store: none", result.Output, StringComparison.Ordinal);
        }
        finally
        {
            DeleteTemporaryDirectory(configFilePath);
            ClearEnvironmentVariables();
        }
    }

    [Fact]
    public void Execute_WhenSetKnownKeyIsProvided_SavesConfigurationValue()
    {
        // Arrange
        ClearEnvironmentVariables();

        var configFilePath = CreateTemporaryConfigFilePath();
        var handler = CreateHandler(configFilePath);

        try
        {
            // Act
            var result = ExecuteAndCaptureOutput(handler, ["config", "set", "chat-provider", "ollama"]);

            // Assert
            Assert.Equal(CliExitCodes.Success, result.ExitCode);
            Assert.Empty(result.Error);

            Assert.Contains("DevMemory configuration updated.", result.Output, StringComparison.Ordinal);
            Assert.Contains($"File: {configFilePath}", result.Output, StringComparison.Ordinal);
            Assert.Contains("chat-provider: ollama", result.Output, StringComparison.Ordinal);

            var storedConfiguration = new AiRuntimeConfigurationStore(configFilePath).Load();

            Assert.Equal("ollama", storedConfiguration.ChatProvider);
        }
        finally
        {
            DeleteTemporaryDirectory(configFilePath);
            ClearEnvironmentVariables();
        }
    }

    [Fact]
    public void Execute_WhenMultipleKnownKeysAreSet_SavesConfigurationValues()
    {
        // Arrange
        ClearEnvironmentVariables();

        var configFilePath = CreateTemporaryConfigFilePath();
        var handler = CreateHandler(configFilePath);

        try
        {
            // Act
            var chatResult = ExecuteAndCaptureOutput(handler, ["config", "set", "chat-provider", "ollama"]);
            var embeddingResult = ExecuteAndCaptureOutput(handler, ["config", "set", "embedding-provider", "ollama"]);
            var vectorStoreResult = ExecuteAndCaptureOutput(handler, ["config", "set", "vector-store", "qdrant"]);
            var chatModelResult = ExecuteAndCaptureOutput(handler, ["config", "set", "ollama-chat-model", "llama3.2"]);
            var embeddingModelResult = ExecuteAndCaptureOutput(handler, ["config", "set", "ollama-embedding-model", "nomic-embed-text"]);
            var collectionResult = ExecuteAndCaptureOutput(handler, ["config", "set", "qdrant-collection", "devmemory_memories"]);

            // Assert
            Assert.Equal(CliExitCodes.Success, chatResult.ExitCode);
            Assert.Equal(CliExitCodes.Success, embeddingResult.ExitCode);
            Assert.Equal(CliExitCodes.Success, vectorStoreResult.ExitCode);
            Assert.Equal(CliExitCodes.Success, chatModelResult.ExitCode);
            Assert.Equal(CliExitCodes.Success, embeddingModelResult.ExitCode);
            Assert.Equal(CliExitCodes.Success, collectionResult.ExitCode);

            var storedConfiguration = new AiRuntimeConfigurationStore(configFilePath).Load();

            Assert.Equal("ollama", storedConfiguration.ChatProvider);
            Assert.Equal("ollama", storedConfiguration.EmbeddingProvider);
            Assert.Equal("qdrant", storedConfiguration.VectorStore);
            Assert.Equal("llama3.2", storedConfiguration.OllamaChatModel);
            Assert.Equal("nomic-embed-text", storedConfiguration.OllamaEmbeddingModel);
            Assert.Equal("devmemory_memories", storedConfiguration.QdrantCollection);
        }
        finally
        {
            DeleteTemporaryDirectory(configFilePath);
            ClearEnvironmentVariables();
        }
    }

    [Fact]
    public void Execute_WhenSetUnknownKeyIsProvided_ReturnsFailureAndPrintsSupportedKeys()
    {
        // Arrange
        ClearEnvironmentVariables();

        var configFilePath = CreateTemporaryConfigFilePath();
        var handler = CreateHandler(configFilePath);

        try
        {
            // Act
            var result = ExecuteAndCaptureOutput(handler, ["config", "set", "unknown-key", "value"]);

            // Assert
            Assert.Equal(CliExitCodes.Failure, result.ExitCode);
            Assert.Empty(result.Output);

            Assert.Contains("Unknown configuration key: unknown-key", result.Error, StringComparison.Ordinal);
            Assert.Contains("Supported keys:", result.Error + result.Output, StringComparison.Ordinal);
            Assert.False(File.Exists(configFilePath));
        }
        finally
        {
            DeleteTemporaryDirectory(configFilePath);
            ClearEnvironmentVariables();
        }
    }

    [Fact]
    public void Execute_WhenSetValueIsMissing_ReturnsFailure()
    {
        // Arrange
        ClearEnvironmentVariables();

        var configFilePath = CreateTemporaryConfigFilePath();
        var handler = CreateHandler(configFilePath);

        try
        {
            // Act
            var result = ExecuteAndCaptureOutput(handler, ["config", "set", "chat-provider"]);

            // Assert
            Assert.Equal(CliExitCodes.Failure, result.ExitCode);
            Assert.Contains("Usage:", result.Output, StringComparison.Ordinal);
            Assert.False(File.Exists(configFilePath));
        }
        finally
        {
            DeleteTemporaryDirectory(configFilePath);
            ClearEnvironmentVariables();
        }
    }

    [Fact]
    public void Execute_WhenResetIsProvided_DeletesPersistedConfiguration()
    {
        // Arrange
        ClearEnvironmentVariables();

        var configFilePath = CreateTemporaryConfigFilePath();
        var handler = CreateHandler(configFilePath);

        try
        {
            ExecuteAndCaptureOutput(handler, ["config", "set", "chat-provider", "ollama"]);

            Assert.True(File.Exists(configFilePath));

            // Act
            var result = ExecuteAndCaptureOutput(handler, ["config", "reset"]);

            // Assert
            Assert.Equal(CliExitCodes.Success, result.ExitCode);
            Assert.Empty(result.Error);

            Assert.Contains("DevMemory configuration reset.", result.Output, StringComparison.Ordinal);
            Assert.Contains($"File: {configFilePath}", result.Output, StringComparison.Ordinal);
            Assert.False(File.Exists(configFilePath));
        }
        finally
        {
            DeleteTemporaryDirectory(configFilePath);
            ClearEnvironmentVariables();
        }
    }

    [Fact]
    public void Execute_WhenInvalidSubCommandIsProvided_ReturnsFailureAndPrintsUsage()
    {
        // Arrange
        ClearEnvironmentVariables();

        var configFilePath = CreateTemporaryConfigFilePath();
        var handler = CreateHandler(configFilePath);

        try
        {
            // Act
            var result = ExecuteAndCaptureOutput(handler, ["config", "invalid"]);

            // Assert
            Assert.Equal(CliExitCodes.Failure, result.ExitCode);
            Assert.Contains("Usage:", result.Output, StringComparison.Ordinal);
            Assert.Contains("devmemory config show", result.Output, StringComparison.Ordinal);
            Assert.Contains("devmemory config set <key> <value>", result.Output, StringComparison.Ordinal);
            Assert.Contains("devmemory config reset", result.Output, StringComparison.Ordinal);
        }
        finally
        {
            DeleteTemporaryDirectory(configFilePath);
            ClearEnvironmentVariables();
        }
    }

    #region Helpers

    private static ConfigCommandHandler CreateHandler(string configFilePath)
    {
        return new ConfigCommandHandler(new AiRuntimeConfigurationStore(configFilePath));
    }

    private static string CreateTemporaryConfigFilePath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "devmemory-config-command-tests",
            Guid.NewGuid().ToString("N"),
            "config.json");
    }

    private static void DeleteTemporaryDirectory(string configFilePath)
    {
        var directoryPath = Path.GetDirectoryName(configFilePath);

        if (!string.IsNullOrWhiteSpace(directoryPath) &&
            Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    private static CommandResult ExecuteAndCaptureOutput(
        ConfigCommandHandler handler,
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

    private static void ClearEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.ChatProvider, null);
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.EmbeddingProvider, null);
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.VectorStore, null);

        Environment.SetEnvironmentVariable(AiEnvironmentVariables.OllamaEndpoint, null);
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.OllamaChatModel, null);
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.OllamaEmbeddingModel, null);

        Environment.SetEnvironmentVariable(AiEnvironmentVariables.QdrantEndpoint, null);
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.QdrantCollection, null);

        Environment.SetEnvironmentVariable(AiEnvironmentVariables.OpenAiApiKey, null);
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.OpenAiChatModel, null);
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.OpenAiEmbeddingModel, null);

        Environment.SetEnvironmentVariable(AiEnvironmentVariables.GeminiApiKey, null);
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.GeminiChatModel, null);
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.GeminiEmbeddingModel, null);

        Environment.SetEnvironmentVariable(AiEnvironmentVariables.AnthropicApiKey, null);
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.AnthropicChatModel, null);
    }

    private sealed record CommandResult(
        int ExitCode,
        string Output,
        string Error);

    #endregion
}
