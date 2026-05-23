using DevMemory.Application.Models.Ai;
using DevMemory.Infrastructure.Ai;
using DevMemory.Infrastructure.Ai.Ollama;

namespace DevMemory.Infrastructure.Tests;

public sealed class EmbeddingServiceFactoryTests
{
    [Fact]
    public void Create_WhenSemanticSearchIsDisabled_ReturnsNull()
    {
        // Arrange
        var options = new AiRuntimeOptions();

        // Act
        var service = EmbeddingServiceFactory.Create(options);

        // Assert
        Assert.Null(service);
    }

    [Fact]
    public void Create_WhenOllamaAndQdrantAreConfigured_ReturnsOllamaEmbeddingService()
    {
        // Arrange
        var options = new AiRuntimeOptions
        {
            Embedding = new EmbeddingProviderOptions
            {
                Provider = AiProviderNames.Ollama,
                OllamaEndpoint = "http://localhost:11434",
                OllamaEmbeddingModel = "nomic-embed-text"
            },
            VectorStore = new VectorStoreOptions
            {
                Provider = VectorStoreNames.Qdrant,
                QdrantEndpoint = "http://localhost:6333",
                QdrantCollection = "devmemory_memories"
            }
        };

        // Act
        var service = EmbeddingServiceFactory.Create(options);

        // Assert
        Assert.IsType<OllamaEmbeddingService>(service);

        if (service is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    [Fact]
    public void Create_WhenCloudEmbeddingProviderIsConfiguredButNotImplemented_ReturnsNull()
    {
        // Arrange
        var options = new AiRuntimeOptions
        {
            Embedding = new EmbeddingProviderOptions
            {
                Provider = AiProviderNames.OpenAi,
                OpenAiApiKey = "test-key",
                OpenAiEmbeddingModel = "text-embedding-test"
            },
            VectorStore = new VectorStoreOptions
            {
                Provider = VectorStoreNames.Qdrant,
                QdrantEndpoint = "http://localhost:6333",
                QdrantCollection = "devmemory_memories"
            }
        };

        // Act
        var service = EmbeddingServiceFactory.Create(options);

        // Assert
        Assert.Null(service);
    }
}
