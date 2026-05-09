namespace AiAgent.Core;

public class MemoryEntry
{
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}