using DevMemory.Core;

namespace DevMemory.Application.Models.Memory;

/// <summary>
/// Represents the result of a memory edit operation.
/// </summary>
public sealed class EditMemoryResult
{
    private EditMemoryResult(
        bool success,
        TaskMemory? memory,
        string? markdownFilePath,
        IReadOnlyCollection<string> errors)
    {
        Success = success;
        Memory = memory;
        MarkdownFilePath = markdownFilePath;
        Errors = errors;
    }

    public bool Success { get; }

    public TaskMemory? Memory { get; }

    public string? MarkdownFilePath { get; }

    public IReadOnlyCollection<string> Errors { get; }

    public static EditMemoryResult Ok(
        TaskMemory memory,
        string markdownFilePath)
    {
        return new EditMemoryResult(true, memory, markdownFilePath, []);
    }

    public static EditMemoryResult Fail(params string[] errors)
    {
        return new EditMemoryResult(false, null, null, errors);
    }

    public static EditMemoryResult Fail(IReadOnlyCollection<string> errors)
    {
        return new EditMemoryResult(false, null, null, errors);
    }
}
