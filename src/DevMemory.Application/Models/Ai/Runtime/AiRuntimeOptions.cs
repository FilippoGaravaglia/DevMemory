using DevMemory.Application.Models.Ai.Chat;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.VectorStore;

namespace DevMemory.Application.Models.Ai.Runtime;

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
