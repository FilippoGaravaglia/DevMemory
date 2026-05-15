using DevMemory.Application.Models.Ai;

namespace DevMemory.Application.Abstractions.Ai;

/// <summary>
/// Defines a provider-independent contract for generating text embeddings.
/// </summary>
public interface IEmbeddingService
{
    Task<EmbeddingResponse> GenerateEmbeddingAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken);
}
