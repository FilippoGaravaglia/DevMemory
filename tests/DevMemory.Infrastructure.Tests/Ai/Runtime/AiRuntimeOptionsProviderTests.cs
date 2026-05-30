using DevMemory.Application.Models.Ai.Runtime;

namespace DevMemory.Infrastructure.Tests.Ai.Runtime;

public sealed class AiRuntimeOptionsProviderTests
{
    [Fact]
    public void GetOptions_WhenEnvironmentVariablesAreNotSet_ReturnsDefaults()
    {
        // Arrange
        ClearEnvironmentVariables();

        // Act
        var options = AiRuntimeOptionsProvider.GetOptions(new AiRuntimeConfiguration());

        // Assert
        Assert.Equal(AiProviderNames.None, options.Chat.Provider);
        Assert.Equal("http://localhost:11434", options.Chat.OllamaEndpoint);
        Assert.Equal("llama3.2", options.Chat.OllamaChatModel);
        Assert.Equal("gpt-4.1-mini", options.Chat.OpenAiChatModel);
        Assert.Equal("gemini-1.5-flash", options.Chat.GeminiChatModel);
        Assert.Equal("claude-3-5-sonnet-latest", options.Chat.AnthropicChatModel);
        Assert.False(options.Chat.IsEnabled);

        Assert.Equal(AiProviderNames.None, options.Embedding.Provider);
        Assert.Equal("http://localhost:11434", options.Embedding.OllamaEndpoint);
        Assert.Equal("nomic-embed-text", options.Embedding.OllamaEmbeddingModel);
        Assert.Equal("text-embedding-3-small", options.Embedding.OpenAiEmbeddingModel);
        Assert.Equal("text-embedding-004", options.Embedding.GeminiEmbeddingModel);
        Assert.False(options.Embedding.IsEnabled);

        Assert.Equal(VectorStoreNames.None, options.VectorStore.Provider);
        Assert.Equal("http://localhost:6333", options.VectorStore.QdrantEndpoint);
        Assert.Equal("devmemory_memories", options.VectorStore.QdrantCollection);
        Assert.False(options.VectorStore.IsEnabled);

        Assert.False(options.IsChatEnabled);
        Assert.False(options.IsSemanticSearchEnabled);
        Assert.False(options.IsFullRagEnabled);
    }

    [Fact]
    public void GetOptions_WhenOllamaAndQdrantEnvironmentVariablesAreSet_ReturnsConfiguredLocalOptions()
    {
        // Arrange
        ClearEnvironmentVariables();

        Environment.SetEnvironmentVariable(AiEnvironmentVariables.ChatProvider, "OLLAMA");
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.EmbeddingProvider, "ollama");
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.VectorStore, "qdrant");

        Environment.SetEnvironmentVariable(AiEnvironmentVariables.OllamaEndpoint, "http://localhost:11434/");
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.OllamaChatModel, "llama3.2:1b");
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.OllamaEmbeddingModel, "nomic-embed-text");

        Environment.SetEnvironmentVariable(AiEnvironmentVariables.QdrantEndpoint, "http://localhost:6333/");
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.QdrantCollection, "devmemory_test");

        try
        {
            // Act
            var options = AiRuntimeOptionsProvider.GetOptions();

            // Assert
            Assert.Equal(AiProviderNames.Ollama, options.Chat.Provider);
            Assert.Equal("http://localhost:11434", options.Chat.OllamaEndpoint);
            Assert.Equal("llama3.2:1b", options.Chat.OllamaChatModel);
            Assert.True(options.Chat.IsEnabled);
            Assert.True(options.Chat.IsOllama);

            Assert.Equal(AiProviderNames.Ollama, options.Embedding.Provider);
            Assert.Equal("http://localhost:11434", options.Embedding.OllamaEndpoint);
            Assert.Equal("nomic-embed-text", options.Embedding.OllamaEmbeddingModel);
            Assert.True(options.Embedding.IsEnabled);
            Assert.True(options.Embedding.IsOllama);

            Assert.Equal(VectorStoreNames.Qdrant, options.VectorStore.Provider);
            Assert.Equal("http://localhost:6333", options.VectorStore.QdrantEndpoint);
            Assert.Equal("devmemory_test", options.VectorStore.QdrantCollection);
            Assert.True(options.VectorStore.IsEnabled);
            Assert.True(options.VectorStore.IsQdrant);

            Assert.True(options.IsChatEnabled);
            Assert.True(options.IsSemanticSearchEnabled);
            Assert.True(options.IsFullRagEnabled);
        }
        finally
        {
            ClearEnvironmentVariables();
        }
    }

    [Fact]
    public void GetOptions_WhenCloudEnvironmentVariablesAreSet_ReturnsConfiguredCloudOptions()
    {
        // Arrange
        ClearEnvironmentVariables();

        Environment.SetEnvironmentVariable(AiEnvironmentVariables.ChatProvider, "openai");
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.EmbeddingProvider, "gemini");

        Environment.SetEnvironmentVariable(AiEnvironmentVariables.OpenAiApiKey, "test-openai-key");
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.OpenAiChatModel, "test-openai-chat");

        Environment.SetEnvironmentVariable(AiEnvironmentVariables.GeminiApiKey, "test-gemini-key");
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.GeminiEmbeddingModel, "test-gemini-embedding");

        Environment.SetEnvironmentVariable(AiEnvironmentVariables.AnthropicApiKey, "test-anthropic-key");
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.AnthropicChatModel, "test-anthropic-chat");

        try
        {
            // Act
            var options = AiRuntimeOptionsProvider.GetOptions();

            // Assert
            Assert.Equal(AiProviderNames.OpenAi, options.Chat.Provider);
            Assert.Equal("test-openai-key", options.Chat.OpenAiApiKey);
            Assert.Equal("test-openai-chat", options.Chat.OpenAiChatModel);
            Assert.True(options.Chat.IsOpenAi);

            Assert.Equal(AiProviderNames.Gemini, options.Embedding.Provider);
            Assert.Equal("test-gemini-key", options.Embedding.GeminiApiKey);
            Assert.Equal("test-gemini-embedding", options.Embedding.GeminiEmbeddingModel);
            Assert.True(options.Embedding.IsGemini);

            Assert.Equal("test-anthropic-key", options.Chat.AnthropicApiKey);
            Assert.Equal("test-anthropic-chat", options.Chat.AnthropicChatModel);
        }
        finally
        {
            ClearEnvironmentVariables();
        }
    }

    /// <summary>
    /// Clears AI/RAG environment variables used by these tests.
    /// </summary>
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

    [Fact]
    public void GetOptions_WhenConfigurationIsProvided_UsesPersistedConfiguration()
    {
        // Arrange
        ClearEnvironmentVariables();

        var configuration = new AiRuntimeConfiguration
        {
            ChatProvider = "ollama",
            EmbeddingProvider = "ollama",
            VectorStore = "qdrant",
            OllamaEndpoint = "http://localhost:11434/",
            OllamaChatModel = "llama3.2:latest",
            OllamaEmbeddingModel = "nomic-embed-text:latest",
            QdrantEndpoint = "http://localhost:6333/",
            QdrantCollection = "devmemory_test"
        };

        // Act
        var options = AiRuntimeOptionsProvider.GetOptions(configuration);

        // Assert
        Assert.Equal(AiProviderNames.Ollama, options.Chat.Provider);
        Assert.Equal("http://localhost:11434", options.Chat.OllamaEndpoint);
        Assert.Equal("llama3.2:latest", options.Chat.OllamaChatModel);
        Assert.True(options.Chat.IsEnabled);

        Assert.Equal(AiProviderNames.Ollama, options.Embedding.Provider);
        Assert.Equal("http://localhost:11434", options.Embedding.OllamaEndpoint);
        Assert.Equal("nomic-embed-text:latest", options.Embedding.OllamaEmbeddingModel);
        Assert.True(options.Embedding.IsEnabled);

        Assert.Equal(VectorStoreNames.Qdrant, options.VectorStore.Provider);
        Assert.Equal("http://localhost:6333", options.VectorStore.QdrantEndpoint);
        Assert.Equal("devmemory_test", options.VectorStore.QdrantCollection);
        Assert.True(options.VectorStore.IsEnabled);

        Assert.True(options.IsFullRagEnabled);
    }

    [Fact]
    public void GetOptions_WhenEnvironmentAndConfigurationAreProvided_EnvironmentVariablesWin()
    {
        // Arrange
        ClearEnvironmentVariables();

        var configuration = new AiRuntimeConfiguration
        {
            ChatProvider = "none",
            EmbeddingProvider = "none",
            VectorStore = "none",
            OllamaChatModel = "configured-chat-model",
            QdrantCollection = "configured_collection"
        };

        Environment.SetEnvironmentVariable(AiEnvironmentVariables.ChatProvider, "ollama");
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.EmbeddingProvider, "ollama");
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.VectorStore, "qdrant");
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.OllamaChatModel, "env-chat-model");
        Environment.SetEnvironmentVariable(AiEnvironmentVariables.QdrantCollection, "env_collection");

        try
        {
            // Act
            var options = AiRuntimeOptionsProvider.GetOptions(configuration);

            // Assert
            Assert.Equal(AiProviderNames.Ollama, options.Chat.Provider);
            Assert.Equal(AiProviderNames.Ollama, options.Embedding.Provider);
            Assert.Equal(VectorStoreNames.Qdrant, options.VectorStore.Provider);
            Assert.Equal("env-chat-model", options.Chat.OllamaChatModel);
            Assert.Equal("env_collection", options.VectorStore.QdrantCollection);
        }
        finally
        {
            ClearEnvironmentVariables();
        }
    }
}
