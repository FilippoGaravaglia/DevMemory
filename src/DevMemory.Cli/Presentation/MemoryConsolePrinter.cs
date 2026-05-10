using DevMemory.Core;

namespace DevMemory.Cli.Presentation;

public static class MemoryConsolePrinter
{
    public static void PrintMemoryDetails(TaskMemory memory)
    {
        Console.WriteLine();
        Console.WriteLine($"# {memory.Title}");
        Console.WriteLine();

        Console.WriteLine($"Id: {memory.Id}");
        Console.WriteLine($"Project: {memory.Project}");
        Console.WriteLine($"Area: {memory.Area}");
        Console.WriteLine($"Branch: {memory.Branch}");
        Console.WriteLine($"Created at: {memory.CreatedAt:u}");
        Console.WriteLine($"Tags: {string.Join(", ", memory.Tags)}");
        Console.WriteLine();

        Console.WriteLine("## Problem");
        Console.WriteLine(memory.Problem);
        Console.WriteLine();

        Console.WriteLine("## Solution");
        Console.WriteLine(memory.Solution);
        Console.WriteLine();

        PrintList("## Decisions", memory.Decisions);
        PrintList("## Files touched", memory.FilesTouched);
        PrintList("## Tests", memory.Tests);

        Console.WriteLine("## Lessons learned");
        Console.WriteLine(string.IsNullOrWhiteSpace(memory.LessonsLearned) ? "-" : memory.LessonsLearned);
    }

    public static void PrintList(string title, IReadOnlyCollection<string> values)
    {
        Console.WriteLine(title);

        if (!values.Any())
        {
            Console.WriteLine("-");
            Console.WriteLine();
            return;
        }

        foreach (var value in values)
        {
            Console.WriteLine($"- {value}");
        }

        Console.WriteLine();
    }
}
