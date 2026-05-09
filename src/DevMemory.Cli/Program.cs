using DevMemory.Application;
using DevMemory.Core;
using DevMemory.Infrastructure;
using DevMemory.Application.Models;

var repository = new MemoryRepository();
var markdownExporter = new MarkdownMemoryExporter();
var service = new MemoryService(repository, markdownExporter);

if (args.Length == 0)
{
    PrintHelp();
    return;
}

var command = args[0].Trim().ToLowerInvariant();

switch (command)
{
    case "add":
        AddMemory(service);
        break;

    case "list":
        ListMemories(service);
        break;

    case "search":
        SearchMemories(service, args);
        break;

    case "show":
        ShowMemory(service, args);
        break;

    case "storage":
        ShowStoragePath(service);
        break;

    case "markdown":
        ShowMarkdownDirectory(service);
        break;

    case "help":
    case "--help":
    case "-h":
        PrintHelp();
        break;

    default:
        Console.WriteLine($"Unknown command: {command}");
        Console.WriteLine();
        PrintHelp();
        break;
}

static void AddMemory(MemoryService service)
{
    Console.WriteLine("Add new task memory");
    Console.WriteLine("-------------------");

    var memory = new TaskMemory
    {
        Title = AskRequired("Title"),
        Project = AskRequired("Project"),
        Area = AskRequired("Area"),
        Branch = AskOptional("Branch"),
        Tags = AskList("Tags comma separated"),
        Problem = AskRequired("Problem"),
        Solution = AskRequired("Solution"),
        Decisions = AskMultilineList("Decisions"),
        FilesTouched = AskMultilineList("Files touched"),
        Tests = AskMultilineList("Tests added/updated"),
        LessonsLearned = AskOptional("Lessons learned")
    };

    var result = service.Add(memory);

    Console.WriteLine();

    if (!result.Success)
    {
        Console.WriteLine("Memory was not saved because validation failed.");

        foreach (var error in result.Errors)
        {
            Console.WriteLine($"- {error}");
        }

        return;
    }

Console.WriteLine("Memory saved successfully.");
Console.WriteLine($"Id: {memory.Id}");
Console.WriteLine($"Markdown: {result.MarkdownFilePath}");
}

static void ListMemories(MemoryService service)
{
    var memories = service.List();

    if (!memories.Any())
    {
        Console.WriteLine("No memories found.");
        return;
    }

    foreach (var memory in memories)
    {
        Console.WriteLine($"{memory.Id} | {memory.CreatedAt:u} | {memory.Project} | {memory.Area} | {memory.Title}");
    }
}

static void SearchMemories(MemoryService service, string[] args)
{
    if (args.Length < 2)
    {
        Console.WriteLine("Search query is required.");
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- search <query> [--project <project>] [--area <area>] [--tag <tag>]");
        return;
    }

    var options = BuildSearchOptions(args);
    var results = service.Search(options);

    if (!results.Any())
    {
        Console.WriteLine("No matching memories found.");
        return;
    }

    foreach (var result in results)
    {
        var memory = result.Memory;
        Console.WriteLine($"{memory.Id} | score:{result.Score} | {memory.Project} | {memory.Area} | {memory.Title}");
    }
}

static MemorySearchOptions BuildSearchOptions(string[] args)
{
    var queryParts = new List<string>();
    string? project = null;
    string? area = null;
    string? tag = null;

    for (var index = 1; index < args.Length; index++)
    {
        var value = args[index];

        if (value.Equals("--project", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
        {
            project = args[++index];
            continue;
        }

        if (value.Equals("--area", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
        {
            area = args[++index];
            continue;
        }

        if (value.Equals("--tag", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
        {
            tag = args[++index];
            continue;
        }

        queryParts.Add(value);
    }

    return new MemorySearchOptions
    {
        Query = string.Join(' ', queryParts),
        Project = project,
        Area = area,
        Tag = tag
    };
}

static void ShowMemory(MemoryService service, string[] args)
{
    if (args.Length < 2)
    {
        Console.WriteLine("Memory id is required.");
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- show <memory-id>");
        return;
    }

    if (!Guid.TryParse(args[1], out var id))
    {
        Console.WriteLine("Invalid memory id.");
        return;
    }

    var memory = service.GetById(id);

    if (memory is null)
    {
        Console.WriteLine("Memory not found.");
        return;
    }

    PrintMemoryDetails(memory);
}

static void PrintMemoryDetails(TaskMemory memory)
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

static string AskRequired(string label)
{
    while (true)
    {
        Console.Write($"{label}: ");
        var value = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(value))
        {
            return value.Trim();
        }

        Console.WriteLine($"{label} is required.");
    }
}

static string AskOptional(string label)
{
    Console.Write($"{label}: ");
    return Console.ReadLine()?.Trim() ?? string.Empty;
}

static List<string> AskList(string label)
{
    Console.Write($"{label}: ");
    var value = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(value))
    {
        return [];
    }

    return value
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToList();
}

static List<string> AskMultilineList(string label)
{
    Console.WriteLine($"{label} - write one item per line. Leave empty line to finish.");

    var values = new List<string>();

    while (true)
    {
        Console.Write("- ");
        var value = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(value))
        {
            break;
        }

        values.Add(value.Trim());
    }

    return values;
}

static void PrintList(string title, IReadOnlyCollection<string> values)
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

static void ShowStoragePath(MemoryService service)
{
    Console.WriteLine(service.GetStorageFilePath());
}

static void ShowMarkdownDirectory(MemoryService service)
{
    Console.WriteLine(service.GetMarkdownDirectoryPath());
}

static void PrintHelp()
{
    Console.WriteLine("DevMemory - Local Developer Memory");
    Console.WriteLine("----------------------------------");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- add");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- list");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- search <query> [--project <project>] [--area <area>] [--tag <tag>]");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- show <memory-id>");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- storage");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- markdown");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- search revision");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- search revision --project LogicalCommon");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- search revision --area Estimate");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- search revision --tag dotnet");
}