using DevMemory.Application.Models.Ai.Chat;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;

namespace DevMemory.Infrastructure;

/// <summary>
/// Reads AI and RAG runtime options from environment variables, persisted configuration and defaults.
/// </summary>
public static class AiRuntimeOptionsProvider
{
    public static AiRuntimeOptions GetOptions()
    {
        return GetOptions(new AiRuntimeConfigurationStore().Load());
    }

    public static AiRuntimeOptions GetOptions(AiRuntimeConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var ollamaEndpoint = NormalizeEndpoint(ReadConfiguredValue(
            AiEnvironmentVariables.OllamaEndpoint,
            configuration.OllamaEndpoint,
            "http://localhost:11434"));

        return new AiRuntimeOptions
        {
            Chat = new ChatProviderOptions
            {
                Provider = NormalizeProvider(ReadConfiguredValue(
                    AiEnvironmentVariables.ChatProvider,
                    configuration.ChatProvider,
                    AiProviderNames.None)),
                OllamaEndpoint = ollamaEndpoint,
                OllamaChatModel = ReadConfiguredValue(
                    AiEnvironmentVariables.OllamaChatModel,
                    configuration.OllamaChatModel,
                    "llama3.2"),
                OpenAiApiKey = ReadOptionalEnvironmentValue(AiEnvironmentVariables.OpenAiApiKey),
                OpenAiChatModel = ReadConfiguredValue(
                    AiEnvironmentVariables.OpenAiChatModel,
                    configuration.OpenAiChatModel,
                    "gpt-4.1-mini"),
                GeminiApiKey = ReadOptionalEnvironmentValue(AiEnvironmentVariables.GeminiApiKey),
                GeminiChatModel = ReadConfiguredValue(
                    AiEnvironmentVariables.GeminiChatModel,
                    configuration.GeminiChatModel,
                    "gemini-1.5-flash"),
                AnthropicApiKey = ReadOptionalEnvironmentValue(AiEnvironmentVariables.AnthropicApiKey),
                AnthropicChatModel = ReadConfiguredValue(
                    AiEnvironmentVariables.AnthropicChatModel,
                    configuration.AnthropicChatModel,
                    "claude-3-5-sonnet-latest")
            },
            Embedding = new EmbeddingProviderOptions
            {
                Provider = NormalizeProvider(ReadConfiguredValue(
                    AiEnvironmentVariables.EmbeddingProvider,
                    configuration.EmbeddingProvider,
                    AiProviderNames.None)),
                OllamaEndpoint = ollamaEndpoint,
                OllamaEmbeddingModel = ReadConfiguredValue(
                    AiEnvironmentVariables.OllamaEmbeddingModel,
                    configuration.OllamaEmbeddingModel,
                    "nomic-embed-text"),
                OpenAiApiKey = ReadOptionalEnvironmentValue(AiEnvironmentVariables.OpenAiApiKey),
                OpenAiEmbeddingModel = ReadConfiguredValue(
                    AiEnvironmentVariables.OpenAiEmbeddingModel,
                    configuration.OpenAiEmbeddingModel,
                    "text-embedding-3-small"),
                GeminiApiKey = ReadOptionalEnvironmentValue(AiEnvironmentVariables.GeminiApiKey),
                GeminiEmbeddingModel = ReadConfiguredValue(
                    AiEnvironmentVariables.GeminiEmbeddingModel,
                    configuration.GeminiEmbeddingModel,
                    "text-embedding-004")
            },
            VectorStore = new VectorStoreOptions
            {
                Provider = NormalizeProvider(ReadConfiguredValue(
                    AiEnvironmentVariables.VectorStore,
                    configuration.VectorStore,
                    VectorStoreNames.None)),
                QdrantEndpoint = NormalizeEndpoint(ReadConfiguredValue(
                    AiEnvironmentVariables.QdrantEndpoint,
                    configuration.QdrantEndpoint,
                    "http://localhost:6333")),
                QdrantCollection = ReadConfiguredValue(
                    AiEnvironmentVariables.QdrantCollection,
                    configuration.QdrantCollection,
                    "devmemory_memories")
            }
        };
    }

    /// <summary>
    /// Reads a value using environment variables first, persisted configuration second and a default fallback last.
    /// </summary>
    private static string ReadConfiguredValue(
        string environmentVariableName,
        string? configuredValue,
        string fallback)
    {
        var environmentValue = Environment.GetEnvironmentVariable(environmentVariableName);

        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            return environmentValue.Trim();
        }

        if (!string.IsNullOrWhiteSpace(configuredValue))
        {
            return configuredValue.Trim();
        }

        return fallback;
    }

    /// <summary>
    /// Reads an optional environment variable value.
    /// </summary>
    private static string? ReadOptionalEnvironmentValue(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);

        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    /// <summary>
    /// Normalizes provider names to lower-case invariant values.
    /// </summary>
    private static string NormalizeProvider(string value)
    {
        return value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Normalizes an HTTP endpoint by removing trailing slashes.
    /// </summary>
    private static string NormalizeEndpoint(string value)
    {
        return value.Trim().TrimEnd('/');
    }
}
