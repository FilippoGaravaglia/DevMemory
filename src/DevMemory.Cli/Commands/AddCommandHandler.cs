using DevMemory.Application;
using DevMemory.Cli.CommandLine;
using DevMemory.Core;

namespace DevMemory.Cli.Commands;

public sealed class AddCommandHandler : ICommandHandler
{
    private readonly MemoryService _memoryService;

    public AddCommandHandler(MemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    public string Name => "add";

    public int Execute(string[] args)
    {
        Console.WriteLine("Add new task memory");
        Console.WriteLine("-------------------");

        var memory = new TaskMemory
        {
            Title = CliPrompt.AskRequired("Title"),
            Project = CliPrompt.AskRequired("Project"),
            Area = CliPrompt.AskRequired("Area"),
            Branch = CliPrompt.AskOptional("Branch"),
            Tags = CliPrompt.AskList("Tags comma separated"),
            Problem = CliPrompt.AskRequired("Problem"),
            Solution = CliPrompt.AskRequired("Solution"),
            Decisions = CliPrompt.AskMultilineList("Decisions"),
            FilesTouched = CliPrompt.AskMultilineList("Files touched"),
            Tests = CliPrompt.AskMultilineList("Tests added/updated"),
            LessonsLearned = CliPrompt.AskOptional("Lessons learned")
        };

        var result = _memoryService.Add(memory);

        Console.WriteLine();

        if (!result.Success)
        {
            Console.Error.WriteLine("Memory was not saved because validation failed.");

            foreach (var error in result.Errors)
            {
                Console.Error.WriteLine($"- {error}");
            }

            return CliExitCodes.Failure;
        }

        Console.WriteLine("Memory saved successfully.");
        Console.WriteLine($"Id: {memory.Id}");
        Console.WriteLine($"Markdown: {result.MarkdownFilePath}");

        return CliExitCodes.Success;
    }
}