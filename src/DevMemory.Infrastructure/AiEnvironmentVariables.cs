namespace DevMemory.Infrastructure;

/// <summary>
/// Defines environment variable names used to configure AI and RAG features.
/// </summary>
public static class AiEnvironmentVariables
{
    public const string ChatProvider = "DEVMEMORY_CHAT_PROVIDER";

    public const string EmbeddingProvider = "DEVMEMORY_EMBEDDING_PROVIDER";

    public const string VectorStore = "DEVMEMORY_VECTOR_STORE";

    public const string OllamaEndpoint = "DEVMEMORY_OLLAMA_ENDPOINT";

    public const string OllamaChatModel = "DEVMEMORY_OLLAMA_CHAT_MODEL";

    public const string OllamaEmbeddingModel = "DEVMEMORY_OLLAMA_EMBEDDING_MODEL";

    public const string QdrantEndpoint = "DEVMEMORY_QDRANT_ENDPOINT";

    public const string QdrantCollection = "DEVMEMORY_QDRANT_COLLECTION";

    public const string OpenAiApiKey = "DEVMEMORY_OPENAI_API_KEY";

    public const string OpenAiChatModel = "DEVMEMORY_OPENAI_CHAT_MODEL";

    public const string OpenAiEmbeddingModel = "DEVMEMORY_OPENAI_EMBEDDING_MODEL";

    public const string GeminiApiKey = "DEVMEMORY_GEMINI_API_KEY";

    public const string GeminiChatModel = "DEVMEMORY_GEMINI_CHAT_MODEL";

    public const string GeminiEmbeddingModel = "DEVMEMORY_GEMINI_EMBEDDING_MODEL";

    public const string AnthropicApiKey = "DEVMEMORY_ANTHROPIC_API_KEY";

    public const string AnthropicChatModel = "DEVMEMORY_ANTHROPIC_CHAT_MODEL";
}
