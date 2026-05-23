using DevMemory.Application;
using DevMemory.Application.Models;
using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Commands;

public sealed class LearnFromGitCommandHandler : ICommandHandler
{
    private readonly MemoryService _memoryService;
    private readonly GitMemoryDraftService _gitMemoryDraftService;

    public LearnFromGitCommandHandler(
        MemoryService memoryService,
        GitMemoryDraftService gitMemoryDraftService)
    {
        _memoryService = memoryService;
        _gitMemoryDraftService = gitMemoryDraftService;
    }

    public string Name => "learn-from-git";

    public int Execute(string[] args)
    {
        var repositoryPath = CommandOptions.ReadPathOption(args) ?? Directory.GetCurrentDirectory();

        var draft = _gitMemoryDraftService.CreateDraft(repositoryPath);
        var snapshot = draft.Snapshot;

        PrintGitContext(snapshot);

        Console.WriteLine();
        Console.WriteLine("Create task memory from Git context");
        Console.WriteLine("-----------------------------------");

        var memory = draft.Memory;

        memory.Title = CliPrompt.AskRequiredWithDefault("Title", memory.Title);
        memory.Project = CliPrompt.AskRequiredWithDefault("Project", memory.Project);
        memory.Area = CliPrompt.AskRequired("Area");
        memory.Branch = CliPrompt.AskOptionalWithDefault("Branch", memory.Branch);
        memory.Tags = CliPrompt.AskList("Tags comma separated");
        memory.Problem = CliPrompt.AskRequired("Problem");
        memory.Solution = CliPrompt.AskRequired("Solution");
        memory.Decisions = CliPrompt.AskMultilineList("Decisions");
        memory.FilesTouched = CliPrompt.AskMultilineListWithDefaults("Files touched", memory.FilesTouched);
        memory.Tests = CliPrompt.AskMultilineList("Tests added/updated");
        memory.LessonsLearned = CliPrompt.AskOptional("Lessons learned");

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

    /// <summary>
    /// Prints the Git snapshot used to prefill a task memory.
    /// </summary>
    private static void PrintGitContext(GitRepositorySnapshot snapshot)
    {
        Console.WriteLine("Git context detected");
        Console.WriteLine("--------------------");
        Console.WriteLine($"Repository: {snapshot.RepositoryPath}");
        Console.WriteLine($"Branch: {snapshot.BranchName}");
        Console.WriteLine($"Last commit: {FormatLastCommit(snapshot)}");
        Console.WriteLine();

        Console.WriteLine("Changed files:");

        if (snapshot.ChangedFiles.Count == 0)
        {
            Console.WriteLine("-");
            return;
        }

        foreach (var file in snapshot.ChangedFiles)
        {
            Console.WriteLine($"- {file}");
        }
    }

    /// <summary>
    /// Formats the latest commit information for console output.
    /// </summary>
    private static string FormatLastCommit(GitRepositorySnapshot snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot.LastCommitHash) &&
            string.IsNullOrWhiteSpace(snapshot.LastCommitMessage))
        {
            return "-";
        }

        return $"{snapshot.LastCommitHash} - {snapshot.LastCommitMessage}";
    }
}
