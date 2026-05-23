namespace DevMemory.Application.Models.Ai;

/// <summary>
/// Represents a provider-independent embedding generation request.
/// </summary>
public sealed record EmbeddingRequest
{
    public string Model { get; init; } = string.Empty;

    public string Text { get; init; } = string.Empty;
}
