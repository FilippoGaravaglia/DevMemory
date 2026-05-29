using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using DevMemory.Application.Models.Ai.VectorStore;
using DevMemory.Core;

namespace DevMemory.Application.Ai.Indexing;

/// <summary>
/// Builds vector memory documents from persisted task memories.
/// </summary>
public static class VectorMemoryDocumentBuilder
{
    private const int MaxIndexedFilesTouched = 50;
    private const int MaxIndexableTextLength = 12_000;

    public static IReadOnlyCollection<VectorMemoryDocument> BuildFromMemories(
        IReadOnlyCollection<TaskMemory> memories)
    {
        ArgumentNullException.ThrowIfNull(memories);

        return memories
            .Select(BuildFromMemory)
            .ToList();
    }

    /// <summary>
    /// Builds a vector memory document from a single task memory.
    /// </summary>
    private static VectorMemoryDocument BuildFromMemory(TaskMemory memory)
    {
        ArgumentNullException.ThrowIfNull(memory);

        var indexableText = BuildIndexableText(memory);

        return new VectorMemoryDocument
        {
            MemoryId = memory.Id,
            DocumentId = BuildDocumentId(memory.Id),
            ContentHash = ComputeContentHash(indexableText),
            Title = memory.Title,
            Project = memory.Project,
            Area = memory.Area,
            Branch = memory.Branch,
            Tags = memory.Tags,
            FilesTouched = memory.FilesTouched,
            Text = indexableText
        };
    }

    /// <summary>
    /// Builds a stable vector document id from the memory id.
    /// </summary>
    private static string BuildDocumentId(Guid memoryId)
    {
        return memoryId.ToString("D");
    }

    /// <summary>
    /// Computes a stable SHA-256 hash for the indexable text.
    /// </summary>
    private static string ComputeContentHash(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var hashBytes = SHA256.HashData(bytes);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Builds the text used to generate embeddings for a task memory.
    /// </summary>
    private static string BuildIndexableText(TaskMemory memory)
    {
        var builder = new StringBuilder();

        AppendSection(builder, "Title", memory.Title);
        AppendSection(builder, "Project", memory.Project);
        AppendSection(builder, "Area", memory.Area);
        AppendSection(builder, "Branch", memory.Branch);
        AppendSection(builder, "Tags", memory.Tags);
        AppendSection(builder, "Problem", memory.Problem);
        AppendSection(builder, "Solution", memory.Solution);
        AppendSection(builder, "Decisions", memory.Decisions);
        AppendSection(builder, "Files touched", memory.FilesTouched, MaxIndexedFilesTouched);
        AppendSection(builder, "Tests", memory.Tests);
        AppendSection(builder, "Lessons learned", memory.LessonsLearned);
        AppendSection(builder, "Created at", memory.CreatedAt.ToString("O", CultureInfo.InvariantCulture));

        return TruncateIndexableText(builder.ToString().Trim());
    }

    /// <summary>
    /// Appends a single text section when the value is not empty.
    /// </summary>
    private static void AppendSection(StringBuilder builder, string title, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        builder
            .Append(title)
            .AppendLine(":")
            .AppendLine(value.Trim())
            .AppendLine();
    }

    /// <summary>
    /// Appends a multi-value text section when at least one value is not empty.
    /// </summary>
    private static void AppendSection(
        StringBuilder builder,
        string title,
        IReadOnlyCollection<string> values)
    {
        AppendSection(builder, title, values, maxItems: null);
    }

    /// <summary>
    /// Appends a multi-value text section with an optional item limit.
    /// </summary>
    private static void AppendSection(
        StringBuilder builder,
        string title,
        IReadOnlyCollection<string> values,
        int? maxItems)
    {
        var cleanedValues = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToList();

        if (cleanedValues.Count == 0)
        {
            return;
        }

        var valuesToAppend = maxItems is null
            ? cleanedValues
            : cleanedValues.Take(maxItems.Value).ToList();

        builder
            .Append(title)
            .AppendLine(":");

        foreach (var value in valuesToAppend)
        {
            builder
                .Append("- ")
                .AppendLine(value);
        }

        if (maxItems is not null && cleanedValues.Count > maxItems.Value)
        {
            builder
                .Append("- ... ")
                .Append((cleanedValues.Count - maxItems.Value).ToString(CultureInfo.InvariantCulture))
                .AppendLine(" more item(s) omitted.");
        }

        builder.AppendLine();
    }

    /// <summary>
    /// Truncates the final indexable text to keep embedding input bounded.
    /// </summary>
    private static string TruncateIndexableText(string text)
    {
        if (text.Length <= MaxIndexableTextLength)
        {
            return text;
        }

        const string suffix = """

        [Indexable text truncated to keep embedding input bounded.]
        """;

        var maxContentLength = MaxIndexableTextLength - suffix.Length;

        if (maxContentLength <= 0)
        {
            return text[..MaxIndexableTextLength];
        }

        return string.Concat(text.AsSpan(0, maxContentLength), suffix);
    }
}
