using DevMemory.Application.Models;
using DevMemory.Cli.CommandLine;
using DevMemory.Infrastructure.Git;

namespace DevMemory.Cli.Commands;

public sealed class GitStatusCommandHandler : ICommandHandler
{
    private readonly GitRepositoryInspector _gitInspector;

    public GitStatusCommandHandler(GitRepositoryInspector gitInspector)
    {
        _gitInspector = gitInspector;
    }

    public string Name => "git-status";

    public int Execute(string[] args)
    {
        var repositoryPath = CommandOptions.ReadPathOption(args) ?? Directory.GetCurrentDirectory();

        var snapshot = _gitInspector.Inspect(repositoryPath);

        Console.WriteLine($"Repository: {snapshot.RepositoryPath}");
        Console.WriteLine($"Branch: {snapshot.BranchName}");
        Console.WriteLine($"Last commit: {FormatLastCommit(snapshot)}");
        Console.WriteLine();

        Console.WriteLine("Changed files:");

        if (!snapshot.ChangedFiles.Any())
        {
            Console.WriteLine("-");
            return CliExitCodes.Success;
        }

        foreach (var file in snapshot.ChangedFiles)
        {
            Console.WriteLine($"- {file}");
        }

        return CliExitCodes.Success;
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
