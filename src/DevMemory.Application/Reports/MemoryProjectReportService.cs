using System.Globalization;
using System.Text;
using DevMemory.Core;

namespace DevMemory.Application.Reports;

public static class MemoryProjectReportService
{
    /// <summary>
    /// Builds a Markdown project report from local memories.
    /// </summary>
    public static MemoryProjectReport BuildReport(
        IEnumerable<TaskMemory> memories,
        string project)
    {
        ArgumentNullException.ThrowIfNull(memories);

        if (string.IsNullOrWhiteSpace(project))
        {
            throw new ArgumentException("Project is required.", nameof(project));
        }

        var normalizedProject = project.Trim();

        var projectMemories = memories
            .Where(memory => memory.Project.Equals(normalizedProject, StringComparison.OrdinalIgnoreCase))
            .OrderBy(memory => memory.CreatedAt)
            .ThenBy(memory => memory.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var areas = BuildDistinctValues(projectMemories.Select(memory => memory.Area));
        var tags = BuildDistinctValues(projectMemories.SelectMany(memory => memory.Tags));
        var filesTouched = BuildDistinctValues(projectMemories.SelectMany(memory => memory.FilesTouched));

        var firstMemoryCreatedAt = projectMemories.Count == 0
            ? (DateTime?)null
            : projectMemories.Min(memory => memory.CreatedAt);

        var lastMemoryCreatedAt = projectMemories.Count == 0
            ? (DateTime?)null
            : projectMemories.Max(memory => memory.CreatedAt);

        var markdown = BuildMarkdown(
            normalizedProject,
            projectMemories,
            areas,
            tags,
            filesTouched,
            firstMemoryCreatedAt,
            lastMemoryCreatedAt);

        return new MemoryProjectReport(
            Project: normalizedProject,
            TotalMemories: projectMemories.Count,
            Areas: areas,
            Tags: tags,
            FilesTouched: filesTouched,
            FirstMemoryCreatedAt: firstMemoryCreatedAt,
            LastMemoryCreatedAt: lastMemoryCreatedAt,
            MarkdownContent: markdown);
    }

    #region Helpers

    /// <summary>
    /// Builds the Markdown representation of the project report.
    /// </summary>
    private static string BuildMarkdown(
        string project,
        List<TaskMemory> memories,
        List<string> areas,
        List<string> tags,
        List<string> filesTouched,
        DateTime? firstMemoryCreatedAt,
        DateTime? lastMemoryCreatedAt)
    {
        var builder = new StringBuilder();

        AppendInvariantLine(builder, $"# DevMemory project report: {project}");
        builder.AppendLine();

        builder.AppendLine("> Generated from local DevMemory memories.");
        builder.AppendLine();

        builder.AppendLine("## Summary");
        builder.AppendLine();
        AppendInvariantLine(builder, $"- Project: `{project}`");
        AppendInvariantLine(builder, $"- Total memories: {memories.Count}");
        AppendInvariantLine(builder, $"- Areas: {areas.Count}");
        AppendInvariantLine(builder, $"- Tags: {tags.Count}");
        AppendInvariantLine(builder, $"- Files touched: {filesTouched.Count}");
        AppendInvariantLine(builder, $"- First memory: {FormatDate(firstMemoryCreatedAt)}");
        AppendInvariantLine(builder, $"- Last memory: {FormatDate(lastMemoryCreatedAt)}");
        builder.AppendLine();

        if (memories.Count == 0)
        {
            builder.AppendLine("## No memories found");
            builder.AppendLine();
            builder.AppendLine("No memories were found for this project.");
            builder.AppendLine();
            builder.AppendLine("Create one with:");
            builder.AppendLine();
            builder.AppendLine("```bash");
            builder.AppendLine("devmemory add");
            builder.AppendLine("```");

            return builder.ToString();
        }

        AppendListSection(builder, "## Areas", areas);
        AppendListSection(builder, "## Tags", tags);
        AppendListSection(builder, "## Files touched", filesTouched);

        builder.AppendLine("## Timeline");
        builder.AppendLine();

        foreach (var memory in memories)
        {
            AppendInvariantLine(builder, $"### {FormatDate(memory.CreatedAt)} — {memory.Title}");
            builder.AppendLine();
            AppendInvariantLine(builder, $"- Id: `{memory.Id}`");
            AppendInvariantLine(builder, $"- Area: `{FormatValue(memory.Area)}`");
            AppendInvariantLine(builder, $"- Branch: `{FormatValue(memory.Branch)}`");

            if (memory.Tags.Count > 0)
            {
                AppendInvariantLine(builder, $"- Tags: {BuildInlineCodeList(memory.Tags)}");
            }

            builder.AppendLine();

            AppendTextSection(builder, "Problem", memory.Problem);
            AppendTextSection(builder, "Solution", memory.Solution);
            AppendBulletSection(builder, "Decisions", memory.Decisions);
            AppendBulletSection(builder, "Files touched", memory.FilesTouched);
            AppendBulletSection(builder, "Tests", memory.Tests);
            AppendTextSection(builder, "Lessons learned", memory.LessonsLearned);

            builder.AppendLine("---");
            builder.AppendLine();
        }

        AppendDecisionSummary(builder, memories);
        AppendLessonSummary(builder, memories);

        builder.AppendLine("## Suggested next actions");
        builder.AppendLine();
        builder.AppendLine("- Review the timeline and consolidate repeated decisions.");
        builder.AppendLine("- Use `devmemory insights` to inspect project-level statistics.");
        builder.AppendLine("- Use `devmemory graph-view` to explore relationships between memories, tags and files.");
        builder.AppendLine("- If local AI is configured, run `devmemory index` and ask RAG questions about this project.");
        builder.AppendLine();

        return builder.ToString();
    }

    /// <summary>
    /// Appends a Markdown section containing a simple bullet list.
    /// </summary>
    private static void AppendListSection(
        StringBuilder builder,
        string title,
        List<string> values)
    {
        builder.AppendLine(title);
        builder.AppendLine();

        if (values.Count == 0)
        {
            builder.AppendLine("-");
            builder.AppendLine();
            return;
        }

        foreach (var value in values)
        {
            AppendInvariantLine(builder, $"- `{value}`");
        }

        builder.AppendLine();
    }

    /// <summary>
    /// Appends a Markdown text subsection when content is available.
    /// </summary>
    private static void AppendTextSection(
        StringBuilder builder,
        string title,
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        AppendInvariantLine(builder, $"#### {title}");
        builder.AppendLine();
        builder.AppendLine(value.Trim());
        builder.AppendLine();
    }

    /// <summary>
    /// Appends a Markdown bullet subsection when values are available.
    /// </summary>
    private static void AppendBulletSection(
        StringBuilder builder,
        string title,
        List<string> values)
    {
        var cleanValues = BuildDistinctValues(values);

        if (cleanValues.Count == 0)
        {
            return;
        }

        AppendInvariantLine(builder, $"#### {title}");
        builder.AppendLine();

        foreach (var value in cleanValues)
        {
            AppendInvariantLine(builder, $"- {value}");
        }

        builder.AppendLine();
    }

    /// <summary>
    /// Appends a cross-memory decision summary.
    /// </summary>
    private static void AppendDecisionSummary(
        StringBuilder builder,
        List<TaskMemory> memories)
    {
        var decisions = BuildDistinctValues(memories.SelectMany(memory => memory.Decisions));

        builder.AppendLine("## Decision summary");
        builder.AppendLine();

        if (decisions.Count == 0)
        {
            builder.AppendLine("No decisions were captured in the selected memories.");
            builder.AppendLine();
            return;
        }

        foreach (var decision in decisions)
        {
            AppendInvariantLine(builder, $"- {decision}");
        }

        builder.AppendLine();
    }

    /// <summary>
    /// Appends a cross-memory lesson summary.
    /// </summary>
    private static void AppendLessonSummary(
        StringBuilder builder,
        List<TaskMemory> memories)
    {
        var lessons = BuildDistinctValues(memories.Select(memory => memory.LessonsLearned));

        builder.AppendLine("## Lessons learned summary");
        builder.AppendLine();

        if (lessons.Count == 0)
        {
            builder.AppendLine("No lessons learned were captured in the selected memories.");
            builder.AppendLine();
            return;
        }

        foreach (var lesson in lessons)
        {
            AppendInvariantLine(builder, $"- {lesson}");
        }

        builder.AppendLine();
    }

    /// <summary>
    /// Builds distinct non-empty text values preserving a stable alphabetical order.
    /// </summary>
    private static List<string> BuildDistinctValues(IEnumerable<string?> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Builds a Markdown inline-code list.
    /// </summary>
    private static string BuildInlineCodeList(IEnumerable<string> values)
    {
        return string.Join(
            ", ",
            values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => FormattableString.Invariant($"`{value.Trim()}`")));
    }

    /// <summary>
    /// Formats a nullable date for Markdown output.
    /// </summary>
    private static string FormatDate(DateTime? value)
    {
        return value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "-";
    }

    /// <summary>
    /// Formats an optional text value.
    /// </summary>
    private static string FormatValue(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "-"
            : value.Trim();
    }

    /// <summary>
    /// Appends an invariant-culture interpolated line to the Markdown builder.
    /// </summary>
    private static void AppendInvariantLine(
        StringBuilder builder,
        FormattableString value)
    {
        builder.AppendLine(FormattableString.Invariant(value));
    }

    #endregion
}
