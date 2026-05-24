using System.Globalization;
using System.Text;
using DevMemory.Application.Models.Ai.Rag;
using DevMemory.Application.Models.Ai.VectorStore;

namespace DevMemory.Application.Ai.Rag;

/// <summary>
/// Builds retrieval-augmented prompts from semantic memory search results.
/// </summary>
public sealed class MemoryRagPromptBuilder
{
    private const int DefaultMaxContextItems = 5;

    public static RagPrompt Build(
        string question,
        IReadOnlyCollection<VectorMemorySearchResult> contextResults,
        int maxContextItems = DefaultMaxContextItems)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("RAG question cannot be empty.", nameof(question));
        }

        ArgumentNullException.ThrowIfNull(contextResults);

        if (maxContextItems <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxContextItems),
                "Max context items must be greater than zero.");
        }

        var selectedContext = contextResults
            .Where(result => !string.IsNullOrWhiteSpace(result.Text))
            .OrderByDescending(result => result.Score)
            .Take(maxContextItems)
            .ToList();

        return new RagPrompt
        {
            SystemPrompt = BuildSystemPrompt(),
            UserPrompt = BuildUserPrompt(question.Trim(), selectedContext),
            ContextItemsCount = selectedContext.Count
        };
    }

    /// <summary>
    /// Builds the system prompt used to constrain the assistant behavior.
    /// </summary>
    private static string BuildSystemPrompt()
    {
        return """
        You are DevMemory, a local-first developer memory assistant.

        Answer the user's question using only the provided memory context.
        If the context is insufficient, say that the available memories do not contain enough information.
        Be precise, technical, and concise.
        Prefer concrete implementation details, decisions, files, tests, and lessons learned when available.
        Do not invent facts that are not present in the context.
        """;
    }

    /// <summary>
    /// Builds the user prompt containing the question and retrieved memory context.
    /// </summary>
    private static string BuildUserPrompt(
        string question,
        List<VectorMemorySearchResult> contextResults)
    {
        var builder = new StringBuilder();

        builder.AppendLine("User question:");
        builder.AppendLine(question);
        builder.AppendLine();

        builder.AppendLine("Retrieved memory context:");
        builder.AppendLine();

        if (contextResults.Count == 0)
        {
            builder.AppendLine("No relevant memory context was found.");
            builder.AppendLine();
            builder.AppendLine("Instruction:");
            builder.AppendLine("Explain that the available memories do not contain enough information to answer.");

            return builder.ToString().Trim();
        }

        for (var index = 0; index < contextResults.Count; index++)
        {
            AppendContextItem(builder, index + 1, contextResults[index]);
        }

        builder.AppendLine("Instruction:");
        builder.AppendLine("Answer the user question using the retrieved memory context above.");

        return builder.ToString().Trim();
    }

    /// <summary>
    /// Appends a single retrieved memory item to the prompt context.
    /// </summary>
    private static void AppendContextItem(
        StringBuilder builder,
        int position,
        VectorMemorySearchResult result)
    {
        builder
            .Append("Memory ")
            .Append(position.ToString(CultureInfo.InvariantCulture))
            .AppendLine(":");

        builder
            .Append("MemoryId: ")
            .AppendLine(result.MemoryId.ToString("D"));

        AppendOptionalLine(builder, "Title", result.Title);
        AppendOptionalLine(builder, "Project", result.Project);
        AppendOptionalLine(builder, "Area", result.Area);

        builder
            .Append("Score: ")
            .AppendLine(result.Score.ToString(CultureInfo.InvariantCulture));

        builder.AppendLine("Text:");
        builder.AppendLine(result.Text.Trim());
        builder.AppendLine();
    }

    /// <summary>
    /// Appends a named line when the value is not empty.
    /// </summary>
    private static void AppendOptionalLine(
        StringBuilder builder,
        string label,
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        builder
            .Append(label)
            .Append(": ")
            .AppendLine(value.Trim());
    }
}
