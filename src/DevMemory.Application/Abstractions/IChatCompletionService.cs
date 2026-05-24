using DevMemory.Application.Models.Ai.Chat;

namespace DevMemory.Application.Abstractions;

/// <summary>
/// Defines a provider-independent contract for generating chat completions.
/// </summary>
public interface IChatCompletionService
{
    Task<ChatCompletionResponse> CompleteAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken);
}
