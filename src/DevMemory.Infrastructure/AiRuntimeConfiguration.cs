namespace DevMemory.Infrastructure;

/// <summary>
/// Represents persisted DevMemory AI/RAG runtime configuration values.
/// </summary>
public sealed class AiRuntimeConfiguration
{
    public string? ChatProvider { get; init; }

    public string? EmbeddingProvider { get; init; }

    public string? VectorStore { get; init; }

    public string? OllamaEndpoint { get; init; }

    public string? OllamaChatModel { get; init; }

    public string? OllamaEmbeddingModel { get; init; }

    public string? QdrantEndpoint { get; init; }

    public string? QdrantCollection { get; init; }

    public string? OpenAiChatModel { get; init; }

    public string? OpenAiEmbeddingModel { get; init; }

    public string? GeminiChatModel { get; init; }

    public string? GeminiEmbeddingModel { get; init; }

    public string? AnthropicChatModel { get; init; }
}
