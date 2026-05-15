using DevMemory.Application.Models.Ai;

namespace DevMemory.Application.Abstractions.Ai;

/// <summary>
/// Defines a provider-independent contract for generating chat completions.
/// </summary>
public interface IChatCompletionService
{
    Task<ChatCompletionResponse> CompleteAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken);
}
