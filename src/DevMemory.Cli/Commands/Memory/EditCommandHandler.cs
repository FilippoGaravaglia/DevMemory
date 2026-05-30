using DevMemory.Application;
using DevMemory.Application.Models.Memory;
using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Commands.Memory;

/// <summary>
/// Edits an existing memory from the primary local storage.
/// </summary>
public sealed class EditCommandHandler : ICommandHandler
{
    private readonly MemoryService _memoryService;

    public EditCommandHandler(MemoryService memoryService)
    {
        _memoryService = memoryService ?? throw new ArgumentNullException(nameof(memoryService));
    }

    public string Name => "edit";

    public int Execute(string[] args)
    {
        if (args.Length < 2)
        {
            PrintUsageToError();

            return CliExitCodes.InvalidCommand;
        }

        if (!Guid.TryParse(args[1], out var memoryId))
        {
            Console.Error.WriteLine("Invalid memory id.");
            Console.Error.WriteLine("The memory id must be a valid GUID.");

            return CliExitCodes.InvalidCommand;
        }

        EditMemoryOptions options;

        try
        {
            options = ParseOptions(args);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine();

            PrintUsageToError();

            return CliExitCodes.InvalidCommand;
        }

        var result = _memoryService.Edit(memoryId, options);

        if (!result.Success || result.Memory is null)
        {
            foreach (var error in result.Errors)
            {
                Console.Error.WriteLine(error);
            }

            return CliExitCodes.Failure;
        }

        Console.WriteLine("Memory updated successfully.");
        Console.WriteLine();
        Console.WriteLine($"MemoryId: {result.Memory.Id:D}");
        Console.WriteLine($"Title: {result.Memory.Title}");
        Console.WriteLine($"Project: {result.Memory.Project}");
        Console.WriteLine($"Area: {result.Memory.Area}");
        Console.WriteLine($"Markdown: {result.MarkdownFilePath}");
        Console.WriteLine();
        Console.WriteLine("Note:");
        Console.WriteLine("  The primary JSON memory and Markdown export have been updated.");
        Console.WriteLine("  If this memory was already indexed, run:");
        Console.WriteLine("    devmemory index --force");
        Console.WriteLine("  to rebuild the derived vector index.");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Parses edit command options from CLI arguments.
    /// </summary>
    private static EditMemoryOptions ParseOptions(string[] args)
    {
        var tagsToAdd = new List<string>();
        var tagsToRemove = new List<string>();
        var decisionsToAdd = new List<string>();
        var decisionsToRemove = new List<string>();
        var filesToAdd = new List<string>();
        var filesToRemove = new List<string>();
        var testsToAdd = new List<string>();
        var testsToRemove = new List<string>();

        string? title = null;
        string? project = null;
        string? area = null;
        string? branch = null;
        string? problem = null;
        string? solution = null;
        string? lessonsLearned = null;

        for (var index = 2; index < args.Length; index++)
        {
            var option = args[index];

            switch (option)
            {
                case "--title":
                    title = ReadRequiredOptionValue(args, ref index, option);
                    break;

                case "--project":
                    project = ReadRequiredOptionValue(args, ref index, option);
                    break;

                case "--area":
                    area = ReadRequiredOptionValue(args, ref index, option);
                    break;

                case "--branch":
                    branch = ReadRequiredOptionValue(args, ref index, option);
                    break;

                case "--problem":
                    problem = ReadRequiredOptionValue(args, ref index, option);
                    break;

                case "--solution":
                    solution = ReadRequiredOptionValue(args, ref index, option);
                    break;

                case "--lessons":
                    lessonsLearned = ReadRequiredOptionValue(args, ref index, option);
                    break;

                case "--add-tag":
                    tagsToAdd.Add(ReadRequiredOptionValue(args, ref index, option));
                    break;

                case "--remove-tag":
                    tagsToRemove.Add(ReadRequiredOptionValue(args, ref index, option));
                    break;

                case "--add-decision":
                    decisionsToAdd.Add(ReadRequiredOptionValue(args, ref index, option));
                    break;

                case "--remove-decision":
                    decisionsToRemove.Add(ReadRequiredOptionValue(args, ref index, option));
                    break;

                case "--add-file":
                    filesToAdd.Add(ReadRequiredOptionValue(args, ref index, option));
                    break;

                case "--remove-file":
                    filesToRemove.Add(ReadRequiredOptionValue(args, ref index, option));
                    break;

                case "--add-test":
                    testsToAdd.Add(ReadRequiredOptionValue(args, ref index, option));
                    break;

                case "--remove-test":
                    testsToRemove.Add(ReadRequiredOptionValue(args, ref index, option));
                    break;

                default:
                    throw new ArgumentException($"Unknown edit option: {option}");
            }
        }

        return new EditMemoryOptions
        {
            Title = title,
            Project = project,
            Area = area,
            Branch = branch,
            Problem = problem,
            Solution = solution,
            LessonsLearned = lessonsLearned,
            TagsToAdd = tagsToAdd,
            TagsToRemove = tagsToRemove,
            DecisionsToAdd = decisionsToAdd,
            DecisionsToRemove = decisionsToRemove,
            FilesToAdd = filesToAdd,
            FilesToRemove = filesToRemove,
            TestsToAdd = testsToAdd,
            TestsToRemove = testsToRemove
        };
    }

    /// <summary>
    /// Reads a required option value from the command arguments.
    /// </summary>
    private static string ReadRequiredOptionValue(
        string[] args,
        ref int index,
        string optionName)
    {
        if (index + 1 >= args.Length ||
            string.IsNullOrWhiteSpace(args[index + 1]) ||
            args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Option {optionName} requires a value.");
        }

        index++;

        return args[index].Trim();
    }

    /// <summary>
    /// Prints command usage to the error stream.
    /// </summary>
    private static void PrintUsageToError()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  devmemory edit <memory-id> [options]");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Options:");
        Console.Error.WriteLine("  --title <value>");
        Console.Error.WriteLine("  --project <value>");
        Console.Error.WriteLine("  --area <value>");
        Console.Error.WriteLine("  --branch <value>");
        Console.Error.WriteLine("  --problem <value>");
        Console.Error.WriteLine("  --solution <value>");
        Console.Error.WriteLine("  --lessons <value>");
        Console.Error.WriteLine("  --add-tag <value>");
        Console.Error.WriteLine("  --remove-tag <value>");
        Console.Error.WriteLine("  --add-decision <value>");
        Console.Error.WriteLine("  --remove-decision <value>");
        Console.Error.WriteLine("  --add-file <value>");
        Console.Error.WriteLine("  --remove-file <value>");
        Console.Error.WriteLine("  --add-test <value>");
        Console.Error.WriteLine("  --remove-test <value>");
    }
}
