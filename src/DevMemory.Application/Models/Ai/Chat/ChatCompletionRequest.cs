namespace DevMemory.Application.Models.Ai;

/// <summary>
/// Represents a provider-independent chat completion request.
/// </summary>
public sealed record ChatCompletionRequest
{
    public string Model { get; init; } = string.Empty;

    public IReadOnlyCollection<ChatCompletionMessage> Messages { get; init; } = [];

    public decimal Temperature { get; init; } = 0.2m;
}
