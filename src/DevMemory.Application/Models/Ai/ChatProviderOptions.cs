namespace DevMemory.Application.Models.Ai;

/// <summary>
/// Represents chat completion provider configuration.
/// </summary>
public sealed class ChatProviderOptions
{
    public string Provider { get; init; } = AiProviderNames.None;

    public string OllamaEndpoint { get; init; } = "http://localhost:11434";

    public string OllamaChatModel { get; init; } = "llama3.2";

    public string? OpenAiApiKey { get; init; }

    public string OpenAiChatModel { get; init; } = "gpt-4.1-mini";

    public string? GeminiApiKey { get; init; }

    public string GeminiChatModel { get; init; } = "gemini-1.5-flash";

    public string? AnthropicApiKey { get; init; }

    public string AnthropicChatModel { get; init; } = "claude-3-5-sonnet-latest";

    public bool IsEnabled =>
        !Provider.Equals(AiProviderNames.None, StringComparison.OrdinalIgnoreCase);

    public bool IsOllama =>
        Provider.Equals(AiProviderNames.Ollama, StringComparison.OrdinalIgnoreCase);

    public bool IsOpenAi =>
        Provider.Equals(AiProviderNames.OpenAi, StringComparison.OrdinalIgnoreCase);

    public bool IsGemini =>
        Provider.Equals(AiProviderNames.Gemini, StringComparison.OrdinalIgnoreCase);

    public bool IsAnthropic =>
        Provider.Equals(AiProviderNames.Anthropic, StringComparison.OrdinalIgnoreCase);
}
