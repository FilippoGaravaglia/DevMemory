using System.Text;
using DevMemory.Application.Abstractions;
using DevMemory.Core;

namespace DevMemory.Infrastructure;

public sealed class MarkdownMemoryExporter : IMemoryExporter
{
    private readonly DevMemoryStorageOptions _options;

    public MarkdownMemoryExporter()
        : this(new DevMemoryStorageOptions())
    {
    }

    public MarkdownMemoryExporter(DevMemoryStorageOptions options)
    {
        _options = options;
        Directory.CreateDirectory(_options.MarkdownDirectoryPath);
    }

    public string Export(TaskMemory memory)
    {
        Directory.CreateDirectory(_options.MarkdownDirectoryPath);

        var fileName = BuildFileName(memory);
        var filePath = Path.Combine(_options.MarkdownDirectoryPath, fileName);

        var markdown = BuildMarkdown(memory);

        File.WriteAllText(filePath, markdown, Encoding.UTF8);

        return filePath;
    }

    private static string BuildMarkdown(TaskMemory memory)
    {
        var builder = new StringBuilder();

        AppendInvariantLine(builder, $"# {memory.Title}");
        builder.AppendLine();

        builder.AppendLine("## Metadata");
        AppendInvariantLine(builder, $"- Id: `{memory.Id}`");
        AppendInvariantLine(builder, $"- Project: {memory.Project}");
        AppendInvariantLine(builder, $"- Area: {memory.Area}");
        AppendInvariantLine(builder, $"- Branch: {FormatOptional(memory.Branch)}");
        AppendInvariantLine(builder, $"- Created at: {memory.CreatedAt:u}");
        AppendInvariantLine(builder, $"- Tags: {FormatListInline(memory.Tags)}");
        builder.AppendLine();

        builder.AppendLine("## Problem");
        builder.AppendLine(FormatBlock(memory.Problem));
        builder.AppendLine();

        builder.AppendLine("## Solution");
        builder.AppendLine(FormatBlock(memory.Solution));
        builder.AppendLine();

        AppendListSection(builder, "Decisions", memory.Decisions);
        AppendListSection(builder, "Files touched", memory.FilesTouched);
        AppendListSection(builder, "Tests", memory.Tests);

        builder.AppendLine("## Lessons learned");
        builder.AppendLine(FormatBlock(memory.LessonsLearned));
        builder.AppendLine();

        builder.AppendLine("## Continuation prompt");
        builder.AppendLine("```text");
        AppendInvariantLine(builder, $"I am continuing the task \"{memory.Title}\".");
        AppendInvariantLine(builder, $"Project: {memory.Project}");
        AppendInvariantLine(builder, $"Area: {memory.Area}");
        builder.AppendLine();
        builder.AppendLine("Use the following context:");
        AppendInvariantLine(builder, $"Problem: {memory.Problem}");
        AppendInvariantLine(builder, $"Solution: {memory.Solution}");
        AppendInvariantLine(builder, $"Key decisions: {FormatListInline(memory.Decisions)}");
        AppendInvariantLine(builder, $"Files touched: {FormatListInline(memory.FilesTouched)}");
        AppendInvariantLine(builder, $"Tests: {FormatListInline(memory.Tests)}");
        AppendInvariantLine(builder, $"Lessons learned: {memory.LessonsLearned}");
        builder.AppendLine();
        builder.AppendLine("Help me continue this work with a pragmatic, enterprise-grade approach.");
        builder.AppendLine("```");

        return builder.ToString();
    }

    private static void AppendListSection(
        StringBuilder builder,
        string title,
        List<string> values)
    {
        AppendInvariantLine(builder, $"## {title}");

        if (values.Count == 0)
        {
            builder.AppendLine("-");
            builder.AppendLine();

            return;
        }

        foreach (var value in values)
        {
            AppendInvariantLine(builder, $"- {value}");
        }

        builder.AppendLine();
    }

    private static string BuildFileName(TaskMemory memory)
    {
        var shortId = memory.Id.ToString("N")[..8];
        var slug = Slugify(memory.Title);

        return $"{shortId}__{slug}.md";
    }

    private static string Slugify(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "untitled-memory";
        }

        var normalized = value.Trim().ToLowerInvariant();
        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                continue;
            }

            if (char.IsWhiteSpace(character) || character is '-' or '_')
            {
                builder.Append('-');
            }
        }

        var slug = builder.ToString();

        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return slug.Trim('-');
    }

    private static string FormatOptional(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private static string FormatBlock(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private static string FormatListInline(List<string> values)
    {
        return values.Count > 0
            ? string.Join(", ", values)
            : "-";
    }

    /// <summary>
    /// Appends an interpolated line using invariant culture.
    /// </summary>
    private static void AppendInvariantLine(StringBuilder builder, FormattableString value)
    {
        builder.AppendLine(FormattableString.Invariant(value));
    }
}
