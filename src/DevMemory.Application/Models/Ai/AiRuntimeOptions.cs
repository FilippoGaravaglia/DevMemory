namespace DevMemory.Application.Models.Ai;

/// <summary>
/// Represents the full AI/RAG runtime configuration.
/// </summary>
public sealed class AiRuntimeOptions
{
    public ChatProviderOptions Chat { get; init; } = new();

    public EmbeddingProviderOptions Embedding { get; init; } = new();

    public VectorStoreOptions VectorStore { get; init; } = new();

    public bool IsChatEnabled => Chat.IsEnabled;

    public bool IsSemanticSearchEnabled => Embedding.IsEnabled && VectorStore.IsEnabled;

    public bool IsFullRagEnabled => Chat.IsEnabled && Embedding.IsEnabled && VectorStore.IsEnabled;
}
