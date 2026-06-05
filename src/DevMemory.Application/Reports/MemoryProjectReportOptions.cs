namespace DevMemory.Application.Reports;

public sealed record MemoryProjectReportOptions(
    string Project,
    string? Area,
    string? Tag,
    DateTime? From,
    DateTime? To);
