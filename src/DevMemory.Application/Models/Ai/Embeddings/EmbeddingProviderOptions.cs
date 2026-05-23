namespace DevMemory.Application.Models.Ai;

/// <summary>
/// Represents embedding provider configuration.
/// </summary>
public sealed class EmbeddingProviderOptions
{
    public string Provider { get; init; } = AiProviderNames.None;

    public string OllamaEndpoint { get; init; } = "http://localhost:11434";

    public string OllamaEmbeddingModel { get; init; } = "nomic-embed-text";

    public string? OpenAiApiKey { get; init; }

    public string OpenAiEmbeddingModel { get; init; } = "text-embedding-3-small";

    public string? GeminiApiKey { get; init; }

    public string GeminiEmbeddingModel { get; init; } = "text-embedding-004";

    public bool IsEnabled =>
        !Provider.Equals(AiProviderNames.None, StringComparison.OrdinalIgnoreCase);

    public bool IsOllama =>
        Provider.Equals(AiProviderNames.Ollama, StringComparison.OrdinalIgnoreCase);

    public bool IsOpenAi =>
        Provider.Equals(AiProviderNames.OpenAi, StringComparison.OrdinalIgnoreCase);

    public bool IsGemini =>
        Provider.Equals(AiProviderNames.Gemini, StringComparison.OrdinalIgnoreCase);
}
