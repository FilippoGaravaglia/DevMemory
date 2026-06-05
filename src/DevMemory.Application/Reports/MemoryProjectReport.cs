namespace DevMemory.Application.Reports;

public sealed record MemoryProjectReport(
    string Project,
    string? Area,
    string? Tag,
    DateTime? From,
    DateTime? To,
    int TotalMemories,
    IReadOnlyList<string> Areas,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> FilesTouched,
    DateTime? FirstMemoryCreatedAt,
    DateTime? LastMemoryCreatedAt,
    string MarkdownContent);
