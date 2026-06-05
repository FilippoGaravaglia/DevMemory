using System.Globalization;
using DevMemory.Core;

namespace DevMemory.Application.Insights;

public static class MemoryInsightsService
{
    private const int DefaultTopItems = 5;

    /// <summary>
    /// Builds aggregated insights from the provided memories.
    /// </summary>
    public static MemoryInsights BuildInsights(IEnumerable<TaskMemory> memories)
    {
        ArgumentNullException.ThrowIfNull(memories);

        var memoryList = memories.ToList();

        var mostActiveProjects = BuildTopCounts(
            memoryList.Select(memory => memory.Project),
            DefaultTopItems);

        var mostCommonAreas = BuildTopCounts(
            memoryList.Select(memory => memory.Area),
            DefaultTopItems);

        var mostUsedTags = BuildTopCounts(
            memoryList.SelectMany(memory => memory.Tags),
            DefaultTopItems);

        var fileReferenceCount = memoryList
            .SelectMany(memory => memory.FilesTouched)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        DateTime? lastMemoryCreatedAt = memoryList.Count == 0
            ? null
            : memoryList.Max(memory => memory.CreatedAt);

        var mostActiveMonth = BuildMostActiveMonth(memoryList);

        var suggestions = BuildSuggestions(
            memoryList,
            mostUsedTags,
            fileReferenceCount);

        return new MemoryInsights(
            TotalMemories: memoryList.Count,
            ProjectCount: CountDistinct(memoryList.Select(memory => memory.Project)),
            AreaCount: CountDistinct(memoryList.Select(memory => memory.Area)),
            TagCount: CountDistinct(memoryList.SelectMany(memory => memory.Tags)),
            FileReferenceCount: fileReferenceCount,
            MostActiveProjects: mostActiveProjects,
            MostCommonAreas: mostCommonAreas,
            MostUsedTags: mostUsedTags,
            LastMemoryCreatedAt: lastMemoryCreatedAt,
            MostActiveMonth: mostActiveMonth,
            Suggestions: suggestions);
    }

    /// <summary>
    /// Builds ordered top count items from text values.
    /// </summary>
    private static List<InsightCountItem> BuildTopCounts(
        IEnumerable<string?> values,
        int limit)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
            .Select(group => new InsightCountItem(
                Name: group.First(),
                Count: group.Count()))
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();
    }

    /// <summary>
    /// Counts distinct non-empty text values.
    /// </summary>
    private static int CountDistinct(IEnumerable<string?> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
    }

    /// <summary>
    /// Returns the most active month in yyyy-MM format.
    /// </summary>
    private static string? BuildMostActiveMonth(List<TaskMemory> memories)
    {
        if (memories.Count == 0)
        {
            return null;
        }

        return memories
            .GroupBy(memory => memory.CreatedAt.ToString("yyyy-MM", CultureInfo.InvariantCulture))
            .Select(group => new
            {
                Month = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .ThenByDescending(item => item.Month, StringComparer.Ordinal)
            .First()
            .Month;
    }

    /// <summary>
    /// Builds practical suggestions based on the current memory set.
    /// </summary>
    private static List<string> BuildSuggestions(
        List<TaskMemory> memories,
        IReadOnlyCollection<InsightCountItem> mostUsedTags,
        int fileReferenceCount)
    {
        if (memories.Count == 0)
        {
            return
            [
                "Create your first memory with `devmemory add`.",
                "Run `devmemory setup --wizard` if you want guided onboarding."
            ];
        }

        var suggestions = new List<string>();

        var memoriesWithoutTags = memories.Count(memory => memory.Tags.Count == 0);
        if (memoriesWithoutTags > 0)
        {
            suggestions.Add($"You have {memoriesWithoutTags} memories without tags: consider improving metadata for better search.");
        }

        if (memories.Count >= 3)
        {
            suggestions.Add("You have enough memories to explore the timeline with `devmemory timeline`.");
        }

        if (fileReferenceCount > 0)
        {
            suggestions.Add("You have memories referencing files: try `devmemory graph-view` to explore relationships.");
        }

        if (memories.Count >= 3 && mostUsedTags.Any(IsAiOrRagTag))
        {
            suggestions.Add("You have AI/RAG-related memories: consider running `devmemory index` for semantic search.");
        }
        else if (memories.Count >= 5)
        {
            suggestions.Add("You have several memories: consider configuring local AI and running `devmemory index`.");
        }

        if (suggestions.Count == 0)
        {
            suggestions.Add("Keep capturing structured memories after meaningful tasks, decisions and bug fixes.");
        }

        return suggestions;
    }

    /// <summary>
    /// Returns true when a tag appears related to AI, RAG, embeddings or vector search.
    /// </summary>
    private static bool IsAiOrRagTag(InsightCountItem item)
    {
        var value = item.Name.Trim();

        return value.Equals("ai", StringComparison.OrdinalIgnoreCase)
            || value.Equals("rag", StringComparison.OrdinalIgnoreCase)
            || value.Equals("llm", StringComparison.OrdinalIgnoreCase)
            || value.Equals("qdrant", StringComparison.OrdinalIgnoreCase)
            || value.Equals("ollama", StringComparison.OrdinalIgnoreCase)
            || value.Equals("embedding", StringComparison.OrdinalIgnoreCase)
            || value.Equals("embeddings", StringComparison.OrdinalIgnoreCase)
            || value.Equals("vector", StringComparison.OrdinalIgnoreCase);
    }
}
