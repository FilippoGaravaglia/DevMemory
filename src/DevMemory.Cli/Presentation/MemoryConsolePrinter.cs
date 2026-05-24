using System.Globalization;
using DevMemory.Application.Models.Memory;
using DevMemory.Core;

namespace DevMemory.Cli.Presentation;

public static class MemoryConsolePrinter
{
    public static void PrintMemoryList(IReadOnlyCollection<TaskMemory> memories)
    {
        if (memories.Count == 0)
        {
            Console.WriteLine("No memories found.");
            Console.WriteLine("Create your first memory with:");
            Console.WriteLine("  devmemory add");

            return;
        }

        Console.WriteLine($"Found {memories.Count} memories");
        Console.WriteLine();

        var index = 1;

        foreach (var memory in memories)
        {
            PrintMemorySummary(index, memory);
            index++;
        }
    }

    public static void PrintSearchResults(IReadOnlyCollection<MemorySearchResult> results)
    {
        if (results.Count == 0)
        {
            Console.WriteLine("No matching memories found.");
            Console.WriteLine("Try using a broader query or removing filters.");

            return;
        }

        Console.WriteLine($"Found {results.Count} matching memories");
        Console.WriteLine();

        var index = 1;

        foreach (var result in results)
        {
            PrintMemorySummary(index, result.Memory, result.Score);
            index++;
        }
    }

    public static void PrintMemoryDetails(TaskMemory memory)
    {
        Console.WriteLine();
        Console.WriteLine(memory.Title);
        Console.WriteLine(new string('=', memory.Title.Length));
        Console.WriteLine();

        PrintField("Id", memory.Id.ToString());
        PrintField("Project", memory.Project);
        PrintField("Area", memory.Area);
        PrintField("Branch", FormatOptional(memory.Branch));
        PrintField("Created", memory.CreatedAt.ToString("u", CultureInfo.InvariantCulture));
        PrintField("Tags", FormatListInline(memory.Tags));
        Console.WriteLine();

        PrintSection("Problem", memory.Problem);
        PrintSection("Solution", memory.Solution);

        PrintList("Decisions", memory.Decisions);
        PrintList("Files touched", memory.FilesTouched);
        PrintList("Tests", memory.Tests);

        PrintSection("Lessons learned", memory.LessonsLearned);
    }

    public static void PrintList(string title, IReadOnlyCollection<string> values)
    {
        Console.WriteLine(title);
        Console.WriteLine(new string('-', title.Length));

        if (values.Count == 0)
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

    private static void PrintMemorySummary(int index, TaskMemory memory, int? score = null)
    {
        var title = string.IsNullOrWhiteSpace(memory.Title)
            ? "(untitled)"
            : memory.Title;

        Console.WriteLine($"[{index}] {title}");

        PrintField("Id", memory.Id.ToString(), indent: 4);
        PrintField("Project", memory.Project, indent: 4);
        PrintField("Area", memory.Area, indent: 4);
        PrintField("Branch", FormatOptional(memory.Branch), indent: 4);
        PrintField("Tags", FormatListInline(memory.Tags), indent: 4);
        PrintField("Created", memory.CreatedAt.ToString("u", CultureInfo.InvariantCulture), indent: 4);

        if (score is not null)
        {
            PrintField("Score", score.Value.ToString(CultureInfo.InvariantCulture), indent: 4);
        }

        Console.WriteLine();
    }

    private static void PrintSection(string title, string value)
    {
        Console.WriteLine(title);
        Console.WriteLine(new string('-', title.Length));
        Console.WriteLine(string.IsNullOrWhiteSpace(value) ? "-" : value);
        Console.WriteLine();
    }

    private static void PrintField(string name, string value, int indent = 0)
    {
        var prefix = new string(' ', indent);
        var formattedValue = string.IsNullOrWhiteSpace(value) ? "-" : value;

        Console.WriteLine($"{prefix}{name,-8}: {formattedValue}");
    }

    private static string FormatOptional(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private static string FormatListInline(List<string> values)
    {
        return values.Count > 0
            ? string.Join(", ", values)
            : "-";
    }
}
