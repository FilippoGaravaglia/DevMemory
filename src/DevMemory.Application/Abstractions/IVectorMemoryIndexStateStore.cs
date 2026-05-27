namespace DevMemory.Application.Abstractions;

/// <summary>
/// Defines optional vector store capabilities for reading indexed document state.
/// </summary>
public interface IVectorMemoryIndexStateStore
{
    Task<string?> TryGetIndexedContentHashAsync(
        string documentId,
        CancellationToken cancellationToken);
}
