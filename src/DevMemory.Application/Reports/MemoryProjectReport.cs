namespace DevMemory.Application.Reports;

public sealed record MemoryProjectReport(
    string Project,
    int TotalMemories,
    IReadOnlyList<string> Areas,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> FilesTouched,
    DateTime? FirstMemoryCreatedAt,
    DateTime? LastMemoryCreatedAt,
    string MarkdownContent);
