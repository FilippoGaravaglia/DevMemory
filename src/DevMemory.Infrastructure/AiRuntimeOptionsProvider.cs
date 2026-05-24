using DevMemory.Application.Models.Ai.Chat;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;

namespace DevMemory.Infrastructure;

/// <summary>
/// Reads AI and RAG runtime options from environment variables.
/// </summary>
public static class AiRuntimeOptionsProvider
{
    public static AiRuntimeOptions GetOptions()
    {
        var ollamaEndpoint = NormalizeEndpoint(ReadEnvironmentValue(
            AiEnvironmentVariables.OllamaEndpoint,
            "http://localhost:11434"));

        return new AiRuntimeOptions
        {
            Chat = new ChatProviderOptions
            {
                Provider = NormalizeProvider(ReadEnvironmentValue(
                    AiEnvironmentVariables.ChatProvider,
                    AiProviderNames.None)),
                OllamaEndpoint = ollamaEndpoint,
                OllamaChatModel = ReadEnvironmentValue(
                    AiEnvironmentVariables.OllamaChatModel,
                    "llama3.2"),
                OpenAiApiKey = ReadOptionalEnvironmentValue(AiEnvironmentVariables.OpenAiApiKey),
                OpenAiChatModel = ReadEnvironmentValue(
                    AiEnvironmentVariables.OpenAiChatModel,
                    "gpt-4.1-mini"),
                GeminiApiKey = ReadOptionalEnvironmentValue(AiEnvironmentVariables.GeminiApiKey),
                GeminiChatModel = ReadEnvironmentValue(
                    AiEnvironmentVariables.GeminiChatModel,
                    "gemini-1.5-flash"),
                AnthropicApiKey = ReadOptionalEnvironmentValue(AiEnvironmentVariables.AnthropicApiKey),
                AnthropicChatModel = ReadEnvironmentValue(
                    AiEnvironmentVariables.AnthropicChatModel,
                    "claude-3-5-sonnet-latest")
            },
            Embedding = new EmbeddingProviderOptions
            {
                Provider = NormalizeProvider(ReadEnvironmentValue(
                    AiEnvironmentVariables.EmbeddingProvider,
                    AiProviderNames.None)),
                OllamaEndpoint = ollamaEndpoint,
                OllamaEmbeddingModel = ReadEnvironmentValue(
                    AiEnvironmentVariables.OllamaEmbeddingModel,
                    "nomic-embed-text"),
                OpenAiApiKey = ReadOptionalEnvironmentValue(AiEnvironmentVariables.OpenAiApiKey),
                OpenAiEmbeddingModel = ReadEnvironmentValue(
                    AiEnvironmentVariables.OpenAiEmbeddingModel,
                    "text-embedding-3-small"),
                GeminiApiKey = ReadOptionalEnvironmentValue(AiEnvironmentVariables.GeminiApiKey),
                GeminiEmbeddingModel = ReadEnvironmentValue(
                    AiEnvironmentVariables.GeminiEmbeddingModel,
                    "text-embedding-004")
            },
            VectorStore = new VectorStoreOptions
            {
                Provider = NormalizeProvider(ReadEnvironmentValue(
                    AiEnvironmentVariables.VectorStore,
                    VectorStoreNames.None)),
                QdrantEndpoint = NormalizeEndpoint(ReadEnvironmentValue(
                    AiEnvironmentVariables.QdrantEndpoint,
                    "http://localhost:6333")),
                QdrantCollection = ReadEnvironmentValue(
                    AiEnvironmentVariables.QdrantCollection,
                    "devmemory_memories")
            }
        };
    }

    /// <summary>
    /// Reads an environment variable value or returns a fallback when it is missing.
    /// </summary>
    private static string ReadEnvironmentValue(string name, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(name);

        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : value.Trim();
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
