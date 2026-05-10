namespace DevMemory.Core;

public sealed class TaskMemory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string Project { get; set; } = string.Empty;

    public string Area { get; set; } = string.Empty;

    public string Branch { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = [];

    public string Problem { get; set; } = string.Empty;

    public string Solution { get; set; } = string.Empty;

    public List<string> Decisions { get; set; } = [];

    public List<string> FilesTouched { get; set; } = [];

    public List<string> Tests { get; set; } = [];

    public string LessonsLearned { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
