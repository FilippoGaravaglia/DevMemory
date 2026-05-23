using DevMemory.Core;

namespace DevMemory.Application.Normalization;

public static class TaskMemoryNormalizer
{
    public static void Normalize(TaskMemory memory)
    {
        memory.Title = NormalizeText(memory.Title);
        memory.Project = NormalizeText(memory.Project);
        memory.Area = NormalizeText(memory.Area);
        memory.Branch = NormalizeText(memory.Branch);
        memory.Problem = NormalizeText(memory.Problem);
        memory.Solution = NormalizeText(memory.Solution);
        memory.LessonsLearned = NormalizeText(memory.LessonsLearned);

        memory.Tags = NormalizeList(memory.Tags)
            .Select(tag => tag.ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        memory.Decisions = NormalizeList(memory.Decisions);
        memory.FilesTouched = NormalizeList(memory.FilesTouched);
        memory.Tests = NormalizeList(memory.Tests);
    }

    private static string NormalizeText(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim();
    }

    private static List<string> NormalizeList(IEnumerable<string> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
