namespace DevMemory.Application.Models.Ai;

/// <summary>
/// Represents vector store configuration.
/// </summary>
public sealed class VectorStoreOptions
{
    public string Provider { get; init; } = VectorStoreNames.None;

    public string QdrantEndpoint { get; init; } = "http://localhost:6333";

    public string QdrantCollection { get; init; } = "devmemory_memories";

    public bool IsEnabled =>
        !Provider.Equals(VectorStoreNames.None, StringComparison.OrdinalIgnoreCase);

    public bool IsQdrant =>
        Provider.Equals(VectorStoreNames.Qdrant, StringComparison.OrdinalIgnoreCase);
}
