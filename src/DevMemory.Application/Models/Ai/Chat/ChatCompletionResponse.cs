namespace DevMemory.Application.Models.Ai;

/// <summary>
/// Represents a provider-independent chat completion response.
/// </summary>
public sealed record ChatCompletionResponse
{
    public string Content { get; init; } = string.Empty;

    public string Provider { get; init; } = AiProviderNames.None;

    public string Model { get; init; } = string.Empty;
}
