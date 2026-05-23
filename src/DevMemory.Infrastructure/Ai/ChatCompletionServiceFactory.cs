using DevMemory.Application.Abstractions.Ai;
using DevMemory.Application.Models.Ai;
using DevMemory.Infrastructure.Ai.Ollama;

namespace DevMemory.Infrastructure.Ai;

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
