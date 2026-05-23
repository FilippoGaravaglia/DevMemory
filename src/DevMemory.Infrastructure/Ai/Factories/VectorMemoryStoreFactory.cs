using DevMemory.Application.Abstractions.Ai;
using DevMemory.Application.Models.Ai;
using DevMemory.Infrastructure.Ai.Qdrant;

namespace DevMemory.Infrastructure.Ai;

public static class VectorMemoryStoreFactory
{
    public static IVectorMemoryStore? Create(AiRuntimeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.VectorStore.IsQdrant)
        {
            return null;
        }

        return new QdrantVectorMemoryStore(
            options.VectorStore.QdrantEndpoint,
            options.VectorStore.QdrantCollection);
    }
}
