namespace DevMemory.Application.Insights;

public sealed record MemoryInsights(
    int TotalMemories,
    int ProjectCount,
    int AreaCount,
    int TagCount,
    int FileReferenceCount,
    IReadOnlyList<InsightCountItem> MostActiveProjects,
    IReadOnlyList<InsightCountItem> MostCommonAreas,
    IReadOnlyList<InsightCountItem> MostUsedTags,
    DateTime? LastMemoryCreatedAt,
    string? MostActiveMonth,
    IReadOnlyList<string> Suggestions);

public sealed record InsightCountItem(
    string Name,
    int Count);
