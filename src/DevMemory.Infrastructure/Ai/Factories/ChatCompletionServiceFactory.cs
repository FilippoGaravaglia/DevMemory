using DevMemory.Application.Abstractions;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Infrastructure.Ai.Ollama;

namespace DevMemory.Infrastructure.Ai.Factories;

public static class ChatCompletionServiceFactory
{
    public static IChatCompletionService? Create(AiRuntimeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.IsChatEnabled)
        {
            return null;
        }

        if (options.Chat.IsOllama)
        {
            return new OllamaChatCompletionService(options.Chat.OllamaEndpoint);
        }

        return null;
    }
}
