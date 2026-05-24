
using DevMemory.Application.Models.Ai.Runtime;

namespace DevMemory.Application.Models.Ai.Embeddings;

/// <summary>
/// Represents a provider-independent embedding generation response.
/// </summary>
public sealed record EmbeddingResponse
{
    public IReadOnlyList<float> Vector { get; init; } = [];

    public string Provider { get; init; } = AiProviderNames.None;

    public string Model { get; init; } = string.Empty;
}
