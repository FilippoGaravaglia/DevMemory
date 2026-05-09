using DevMemory.Application;
using DevMemory.Application.Models;
using DevMemory.Core;
using DevMemory.Infrastructure;
using DevMemory.Infrastructure.Git;

var repository = new MemoryRepository();
var markdownExporter = new MarkdownMemoryExporter();
var service = new MemoryService(repository, markdownExporter);
var gitInspector = new GitRepositoryInspector();
var gitMemoryDraftService = new GitMemoryDraftService(gitInspector);

if (args.Length == 0)
{
    PrintHelp();
    return 2;
}

var command = args[0].Trim().ToLowerInvariant();

try
{
    return command switch
    {
        "add" => AddMemory(service),
        "list" => ListMemories(service),
        "search" => SearchMemories(service, args),
        "show" => ShowMemory(service, args),
        "storage" => ShowStoragePath(service),
        "markdown" => ShowMarkdownDirectory(service),
        "git-status" => ShowGitStatus(gitInspector, args),
        "learn-from-git" => LearnFromGit(service, gitMemoryDraftService, args),
        "help" or "--help" or "-h" => PrintHelpAndReturnSuccess(),
        _ => HandleUnknownCommand(command)
    };
}
catch (Exception ex)
{
    Console.Error.WriteLine("Unexpected error.");
    Console.Error.WriteLine(ex.Message);
    return 1;
}

static int AddMemory(MemoryService service)
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
        Console.Error.WriteLine("Memory was not saved because validation failed.");

        foreach (var error in result.Errors)
        {
            Console.Error.WriteLine($"- {error}");
        }

        return 1;
    }

    Console.WriteLine("Memory saved successfully.");
    Console.WriteLine($"Id: {memory.Id}");
    Console.WriteLine($"Markdown: {result.MarkdownFilePath}");

    return 0;
}

static int ListMemories(MemoryService service)
{
    var memories = service.List();

    if (!memories.Any())
    {
        Console.WriteLine("No memories found.");
        return 0;
    }

    foreach (var memory in memories)
    {
        Console.WriteLine($"{memory.Id} | {memory.CreatedAt:u} | {memory.Project} | {memory.Area} | {memory.Title}");
    }

    return 0;
}

static int SearchMemories(MemoryService service, string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Search query is required.");
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  dotnet run --project src/DevMemory.Cli -- search <query> [--project <project>] [--area <area>] [--tag <tag>]");
        return 2;
    }

    var options = BuildSearchOptions(args);

    if (string.IsNullOrWhiteSpace(options.Query))
    {
        Console.Error.WriteLine("Search query is required.");
        return 2;
    }

    var results = service.Search(options);

    if (!results.Any())
    {
        Console.WriteLine("No matching memories found.");
        return 0;
    }

    foreach (var result in results)
    {
        var memory = result.Memory;
        Console.WriteLine($"{memory.Id} | score:{result.Score} | {memory.Project} | {memory.Area} | {memory.Title}");
    }

    return 0;
}

static int ShowMemory(MemoryService service, string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Memory id is required.");
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  dotnet run --project src/DevMemory.Cli -- show <memory-id>");
        return 2;
    }

    if (!Guid.TryParse(args[1], out var id))
    {
        Console.Error.WriteLine("Invalid memory id.");
        return 2;
    }

    var memory = service.GetById(id);

    if (memory is null)
    {
        Console.Error.WriteLine("Memory not found.");
        return 1;
    }

    PrintMemoryDetails(memory);
    return 0;
}

static int ShowStoragePath(MemoryService service)
{
    Console.WriteLine(service.GetStorageFilePath());
    return 0;
}

static int ShowMarkdownDirectory(MemoryService service)
{
    Console.WriteLine(service.GetMarkdownDirectoryPath());
    return 0;
}

static int HandleUnknownCommand(string command)
{
    Console.Error.WriteLine($"Unknown command: {command}");
    Console.Error.WriteLine();
    PrintHelp();
    return 2;
}

static int PrintHelpAndReturnSuccess()
{
    PrintHelp();
    return 0;
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

        if (value.Equals("--project", StringComparison.OrdinalIgnoreCase))
        {
            project = ReadOptionValue(args, ref index, "--project");
            continue;
        }

        if (value.Equals("--area", StringComparison.OrdinalIgnoreCase))
        {
            area = ReadOptionValue(args, ref index, "--area");
            continue;
        }

        if (value.Equals("--tag", StringComparison.OrdinalIgnoreCase))
        {
            tag = ReadOptionValue(args, ref index, "--tag");
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

static string? ReadOptionValue(string[] args, ref int index, string optionName)
{
    if (index + 1 >= args.Length)
    {
        throw new ArgumentException($"Option {optionName} requires a value.");
    }

    var value = args[++index];

    if (value.StartsWith("--", StringComparison.Ordinal))
    {
        throw new ArgumentException($"Option {optionName} requires a value.");
    }

    return value;
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

static int ShowGitStatus(GitRepositoryInspector gitInspector, string[] args)
{
    var repositoryPath = ReadPathOption(args) ?? Directory.GetCurrentDirectory();

    var snapshot = gitInspector.Inspect(repositoryPath);

    Console.WriteLine($"Repository: {snapshot.RepositoryPath}");
    Console.WriteLine($"Branch: {snapshot.BranchName}");
    Console.WriteLine($"Last commit: {FormatLastCommit(snapshot)}");
    Console.WriteLine();

    Console.WriteLine("Changed files:");

    if (!snapshot.ChangedFiles.Any())
    {
        Console.WriteLine("-");
        return 0;
    }

    foreach (var file in snapshot.ChangedFiles)
    {
        Console.WriteLine($"- {file}");
    }

    return 0;
}

static string? ReadPathOption(string[] args)
{
    for (var index = 1; index < args.Length; index++)
    {
        var value = args[index];

        if (value.Equals("--path", StringComparison.OrdinalIgnoreCase))
        {
            return ReadOptionValue(args, ref index, "--path");
        }
    }

    return null;
}

static string FormatLastCommit(DevMemory.Application.Models.GitRepositorySnapshot snapshot)
{
    if (string.IsNullOrWhiteSpace(snapshot.LastCommitHash) &&
        string.IsNullOrWhiteSpace(snapshot.LastCommitMessage))
    {
        return "-";
    }

    return $"{snapshot.LastCommitHash} - {snapshot.LastCommitMessage}";
}

static int LearnFromGit(
    MemoryService memoryService,
    GitMemoryDraftService gitMemoryDraftService,
    string[] args)
{
    var repositoryPath = ReadPathOption(args) ?? Directory.GetCurrentDirectory();

    var draft = gitMemoryDraftService.CreateDraft(repositoryPath);
    var snapshot = draft.Snapshot;

    Console.WriteLine("Git context detected");
    Console.WriteLine("--------------------");
    Console.WriteLine($"Repository: {snapshot.RepositoryPath}");
    Console.WriteLine($"Branch: {snapshot.BranchName}");
    Console.WriteLine($"Last commit: {FormatLastCommit(snapshot)}");
    Console.WriteLine();

    Console.WriteLine("Changed files:");

    if (!snapshot.ChangedFiles.Any())
    {
        Console.WriteLine("-");
    }
    else
    {
        foreach (var file in snapshot.ChangedFiles)
        {
            Console.WriteLine($"- {file}");
        }
    }

    Console.WriteLine();
    Console.WriteLine("Create task memory from Git context");
    Console.WriteLine("-----------------------------------");

    var memory = draft.Memory;

    memory.Title = AskRequiredWithDefault("Title", memory.Title);
    memory.Project = AskRequiredWithDefault("Project", memory.Project);
    memory.Area = AskRequired("Area");
    memory.Branch = AskOptionalWithDefault("Branch", memory.Branch);
    memory.Tags = AskList("Tags comma separated");
    memory.Problem = AskRequired("Problem");
    memory.Solution = AskRequired("Solution");
    memory.Decisions = AskMultilineList("Decisions");
    memory.FilesTouched = AskMultilineListWithDefaults("Files touched", memory.FilesTouched);
    memory.Tests = AskMultilineList("Tests added/updated");
    memory.LessonsLearned = AskOptional("Lessons learned");

    var result = memoryService.Add(memory);

    Console.WriteLine();

    if (!result.Success)
    {
        Console.Error.WriteLine("Memory was not saved because validation failed.");

        foreach (var error in result.Errors)
        {
            Console.Error.WriteLine($"- {error}");
        }

        return 1;
    }

    Console.WriteLine("Memory saved successfully.");
    Console.WriteLine($"Id: {memory.Id}");
    Console.WriteLine($"Markdown: {result.MarkdownFilePath}");

    return 0;
}

static string AskRequiredWithDefault(string label, string defaultValue)
{
    while (true)
    {
        Console.Write($"{label} [{defaultValue}]: ");
        var value = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(value))
        {
            return value.Trim();
        }

        if (!string.IsNullOrWhiteSpace(defaultValue))
        {
            return defaultValue.Trim();
        }

        Console.WriteLine($"{label} is required.");
    }
}

static string AskOptionalWithDefault(string label, string defaultValue)
{
    Console.Write($"{label} [{defaultValue}]: ");
    var value = Console.ReadLine();

    return string.IsNullOrWhiteSpace(value)
        ? defaultValue.Trim()
        : value.Trim();
}

static List<string> AskMultilineListWithDefaults(string label, IReadOnlyCollection<string> defaultValues)
{
    if (defaultValues.Any())
    {
        Console.WriteLine($"{label} detected from Git:");

        foreach (var value in defaultValues)
        {
            Console.WriteLine($"- {value}");
        }

        Console.Write("Use detected values? [Y/n]: ");
        var answer = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(answer) ||
            answer.Equals("y", StringComparison.OrdinalIgnoreCase) ||
            answer.Equals("yes", StringComparison.OrdinalIgnoreCase))
        {
            return defaultValues.ToList();
        }
    }

    return AskMultilineList(label);
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
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- git-status [--path <repository-path>]");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- learn-from-git [--path <repository-path>]");
    Console.WriteLine();
    Console.WriteLine("Installed tool usage:");
    Console.WriteLine("  devmemory add");
    Console.WriteLine("  devmemory list");
    Console.WriteLine("  devmemory search <query> [--project <project>] [--area <area>] [--tag <tag>]");
    Console.WriteLine("  devmemory show <memory-id>");
    Console.WriteLine("  devmemory storage");
    Console.WriteLine("  devmemory markdown");
    Console.WriteLine("  devmemory git-status [--path <repository-path>]");
    Console.WriteLine("  devmemory learn-from-git [--path <repository-path>]");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- search revision");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- search revision --project LogicalCommon");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- search revision --area Estimate");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- search revision --tag dotnet");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- git-status");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- git-status --path ~/work/LogicalCommon");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- learn-from-git");
    Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- learn-from-git --path ~/work/LogicalCommon");
    Console.WriteLine();
    Console.WriteLine("Installed tool examples:");
    Console.WriteLine("  devmemory search revision");
    Console.WriteLine("  devmemory git-status");
    Console.WriteLine("  devmemory git-status --path ~/work/LogicalCommon");
    Console.WriteLine("  devmemory learn-from-git");
    Console.WriteLine("  devmemory learn-from-git --path ~/work/LogicalCommon");
    Console.WriteLine();
    Console.WriteLine("Environment variables:");
    Console.WriteLine("  DEVMEMORY_HOME  Custom DevMemory storage directory");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine("  DEVMEMORY_HOME=~/devmemory-work dotnet run --project src/DevMemory.Cli -- storage");
    Console.WriteLine("  DEVMEMORY_HOME=~/devmemory-work devmemory storage");
}