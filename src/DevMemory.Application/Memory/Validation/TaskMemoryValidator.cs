using DevMemory.Core;

namespace DevMemory.Application.Validation;

public static class TaskMemoryValidator
{
    public static List<string> Validate(TaskMemory memory)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(memory.Title))
        {
            errors.Add("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(memory.Project))
        {
            errors.Add("Project is required.");
        }

        if (string.IsNullOrWhiteSpace(memory.Area))
        {
            errors.Add("Area is required.");
        }

        if (string.IsNullOrWhiteSpace(memory.Problem))
        {
            errors.Add("Problem is required.");
        }

        if (string.IsNullOrWhiteSpace(memory.Solution))
        {
            errors.Add("Solution is required.");
        }

        if (memory.Title.Length > 200)
        {
            errors.Add("Title cannot exceed 200 characters.");
        }

        if (memory.Project.Length > 100)
        {
            errors.Add("Project cannot exceed 100 characters.");
        }

        if (memory.Area.Length > 100)
        {
            errors.Add("Area cannot exceed 100 characters.");
        }

        return errors;
    }
}
