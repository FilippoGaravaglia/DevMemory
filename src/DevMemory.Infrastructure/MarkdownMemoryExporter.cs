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

        builder.AppendLine($"# {memory.Title}");
        builder.AppendLine();

        builder.AppendLine("## Metadata");
        builder.AppendLine($"- Id: `{memory.Id}`");
        builder.AppendLine($"- Project: {memory.Project}");
        builder.AppendLine($"- Area: {memory.Area}");
        builder.AppendLine($"- Branch: {FormatOptional(memory.Branch)}");
        builder.AppendLine($"- Created at: {memory.CreatedAt:u}");
        builder.AppendLine($"- Tags: {FormatListInline(memory.Tags)}");
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
        builder.AppendLine($"I am continuing the task \"{memory.Title}\".");
        builder.AppendLine($"Project: {memory.Project}");
        builder.AppendLine($"Area: {memory.Area}");
        builder.AppendLine();
        builder.AppendLine("Use the following context:");
        builder.AppendLine($"Problem: {memory.Problem}");
        builder.AppendLine($"Solution: {memory.Solution}");
        builder.AppendLine($"Key decisions: {FormatListInline(memory.Decisions)}");
        builder.AppendLine($"Files touched: {FormatListInline(memory.FilesTouched)}");
        builder.AppendLine($"Tests: {FormatListInline(memory.Tests)}");
        builder.AppendLine($"Lessons learned: {memory.LessonsLearned}");
        builder.AppendLine();
        builder.AppendLine("Help me continue this work with a pragmatic, enterprise-grade approach.");
        builder.AppendLine("```");

        return builder.ToString();
    }

    private static void AppendListSection(StringBuilder builder, string title, IReadOnlyCollection<string> values)
    {
        builder.AppendLine($"## {title}");

        if (!values.Any())
        {
            builder.AppendLine("-");
            builder.AppendLine();
            return;
        }

        foreach (var value in values)
        {
            builder.AppendLine($"- {value}");
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

    private static string FormatListInline(IReadOnlyCollection<string> values)
    {
        return values.Any()
            ? string.Join(", ", values)
            : "-";
    }
}
