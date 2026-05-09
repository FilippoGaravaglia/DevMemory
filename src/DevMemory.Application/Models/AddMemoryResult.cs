namespace DevMemory.Application.Models;

public sealed class AddMemoryResult
{
    public bool Success => Errors.Count == 0;

    public string? MarkdownFilePath { get; init; }

    public List<string> Errors { get; init; } = [];

    public static AddMemoryResult Ok(string markdownFilePath)
    {
        return new AddMemoryResult
        {
            MarkdownFilePath = markdownFilePath
        };
    }

    public static AddMemoryResult Fail(IEnumerable<string> errors)
    {
        return new AddMemoryResult
        {
            Errors = errors.ToList()
        };
    }
}