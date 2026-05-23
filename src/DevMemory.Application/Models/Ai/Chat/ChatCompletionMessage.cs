namespace DevMemory.Application.Models.Ai;

/// <summary>
/// Represents a single chat message exchanged with an AI provider.
/// </summary>
public sealed record ChatCompletionMessage
{
    public string Role { get; init; } = ChatMessageRoles.User;

    public string Content { get; init; } = string.Empty;
}
