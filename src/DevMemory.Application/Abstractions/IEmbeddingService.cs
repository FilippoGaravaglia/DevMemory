using DevMemory.Application.Models.Ai.Embeddings;

namespace DevMemory.Application.Abstractions;

/// <summary>
/// Defines a provider-independent contract for generating text embeddings.
/// </summary>
public interface IEmbeddingService
{
    Task<EmbeddingResponse> GenerateEmbeddingAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken);
}
