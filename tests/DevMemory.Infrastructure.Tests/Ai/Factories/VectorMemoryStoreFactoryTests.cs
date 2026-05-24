using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;
using DevMemory.Infrastructure.Ai.Factories;
using DevMemory.Infrastructure.Ai.Qdrant;

namespace DevMemory.Infrastructure.Tests.Ai.Factories;

public sealed class VectorMemoryStoreFactoryTests
{
    [Fact]
    public void Create_WhenVectorStoreIsDisabled_ReturnsNull()
    {
        // Arrange
        var options = new AiRuntimeOptions();

        // Act
        var store = VectorMemoryStoreFactory.Create(options);

        // Assert
        Assert.Null(store);
    }

    [Fact]
    public void Create_WhenQdrantIsConfigured_ReturnsQdrantVectorMemoryStore()
    {
        // Arrange
        var options = new AiRuntimeOptions
        {
            VectorStore = new VectorStoreOptions
            {
                Provider = VectorStoreNames.Qdrant,
                QdrantEndpoint = "http://localhost:6333",
                QdrantCollection = "devmemory_memories"
            }
        };

        // Act
        var store = VectorMemoryStoreFactory.Create(options);

        // Assert
        Assert.IsType<QdrantVectorMemoryStore>(store);

        if (store is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
