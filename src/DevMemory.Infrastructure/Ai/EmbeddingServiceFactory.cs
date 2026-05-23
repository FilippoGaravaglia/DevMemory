using DevMemory.Application.Abstractions.Ai;
using DevMemory.Application.Models.Ai;
using DevMemory.Infrastructure.Ai.Ollama;

namespace DevMemory.Infrastructure.Ai;

public static class EmbeddingServiceFactory
{
    public static IEmbeddingService? Create(AiRuntimeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.IsSemanticSearchEnabled)
        {
            return null;
        }

        if (options.Embedding.IsOllama)
        {
            return new OllamaEmbeddingService(options.Embedding.OllamaEndpoint);
        }

        return null;
    }
}
