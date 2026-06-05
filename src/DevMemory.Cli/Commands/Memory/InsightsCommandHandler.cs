using System.Globalization;
using DevMemory.Application;
using DevMemory.Application.Insights;
using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Commands.Memory;

public sealed class InsightsCommandHandler : ICommandHandler
{
    private readonly MemoryService _memoryService;

    public InsightsCommandHandler(MemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    public string Name => "insights";

    /// <summary>
    /// Executes the insights command.
    /// </summary>
    public int Execute(string[] args)
    {
        if (args.Length > 1)
        {
            var option = args[1];

            if (option is "--help" or "-h")
            {
                PrintUsage();

                return CliExitCodes.Success;
            }

            Console.Error.WriteLine($"Unknown option for insights command: {option}");
            Console.Error.WriteLine();
            PrintUsageToError();

            return CliExitCodes.InvalidCommand;
        }

        var memories = _memoryService.List();
        var insights = MemoryInsightsService.BuildInsights(memories);

        PrintInsights(insights);

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Prints memory insights.
    /// </summary>
    private static void PrintInsights(MemoryInsights insights)
    {
        Console.WriteLine("DevMemory insights");
        Console.WriteLine("------------------");
        Console.WriteLine();

        Console.WriteLine($"Total memories: {insights.TotalMemories}");
        Console.WriteLine($"Projects: {insights.ProjectCount}");
        Console.WriteLine($"Areas: {insights.AreaCount}");
        Console.WriteLine($"Tags: {insights.TagCount}");
        Console.WriteLine($"Files referenced: {insights.FileReferenceCount}");
        Console.WriteLine();

        if (insights.TotalMemories == 0)
        {
            Console.WriteLine("No memories found.");
            Console.WriteLine();
            PrintSuggestions(insights.Suggestions);
            return;
        }

        PrintTopSection("Most active projects:", insights.MostActiveProjects, "memories");
        PrintTopSection("Most common areas:", insights.MostCommonAreas, "memories");
        PrintTopSection("Most used tags:", insights.MostUsedTags, "uses");

        Console.WriteLine("Recent activity:");
        Console.WriteLine($"Last memory: {FormatDate(insights.LastMemoryCreatedAt)}");
        Console.WriteLine($"Most active month: {insights.MostActiveMonth ?? "-"}");
        Console.WriteLine();

        PrintSuggestions(insights.Suggestions);
    }

    /// <summary>
    /// Prints a ranked section.
    /// </summary>
    private static void PrintTopSection(
        string title,
        IReadOnlyList<InsightCountItem> items,
        string countLabel)
    {
        Console.WriteLine(title);

        if (items.Count == 0)
        {
            Console.WriteLine("-");
            Console.WriteLine();
            return;
        }

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            Console.WriteLine($"{i + 1}. {item.Name} - {item.Count} {countLabel}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Prints insight suggestions.
    /// </summary>
    private static void PrintSuggestions(IReadOnlyList<string> suggestions)
    {
        Console.WriteLine("Suggestions:");

        if (suggestions.Count == 0)
        {
            Console.WriteLine("-");
            return;
        }

        foreach (var suggestion in suggestions)
        {
            Console.WriteLine($"- {suggestion}");
        }
    }

    /// <summary>
    /// Formats a nullable date for CLI output.
    /// </summary>
    private static string FormatDate(DateTime? value)
    {
        return value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "-";
    }

    /// <summary>
    /// Prints command usage.
    /// </summary>
    private static void PrintUsage()
    {
        Console.WriteLine("DevMemory insights");
        Console.WriteLine("------------------");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  devmemory insights");
        Console.WriteLine();
        Console.WriteLine("Description:");
        Console.WriteLine("  Shows aggregated statistics and suggestions based on local memories.");
        Console.WriteLine();
        Console.WriteLine("Notes:");
        Console.WriteLine("  This command does not require AI, Ollama or Qdrant.");
        Console.WriteLine("  It reads local JSON storage and does not modify data.");
    }

    /// <summary>
    /// Prints command usage to stderr.
    /// </summary>
    private static void PrintUsageToError()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  devmemory insights");
        Console.Error.WriteLine("  devmemory insights --help");
    }
}
