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
        AppendSection(builder, "Files touched", memory.FilesTouched);
        AppendSection(builder, "Tests", memory.Tests);
        AppendSection(builder, "Lessons learned", memory.LessonsLearned);
        AppendSection(builder, "Created at", memory.CreatedAt.ToString("O", CultureInfo.InvariantCulture));

        return builder.ToString().Trim();
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
        var cleanedValues = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToList();

        if (cleanedValues.Count == 0)
        {
            return;
        }

        builder
            .Append(title)
            .AppendLine(":");

        foreach (var value in cleanedValues)
        {
            builder
                .Append("- ")
                .AppendLine(value);
        }

        builder.AppendLine();
    }
}
