using DevMemory.Application.Abstractions.Git;
using DevMemory.Application.Models.Git;
using DevMemory.Core;

namespace DevMemory.Application;

public sealed class GitMemoryDraftService
{
    private readonly IGitRepositoryInspector _gitRepositoryInspector;

    public GitMemoryDraftService(IGitRepositoryInspector gitRepositoryInspector)
    {
        _gitRepositoryInspector = gitRepositoryInspector;
    }

    public GitMemoryDraft CreateDraft(string repositoryPath)
    {
        var snapshot = _gitRepositoryInspector.Inspect(repositoryPath);

        var memory = new TaskMemory
        {
            Title = BuildTitleFromBranch(snapshot.BranchName),
            Project = ResolveProjectName(snapshot.RepositoryPath),
            Branch = snapshot.BranchName,
            FilesTouched = snapshot.ChangedFiles
        };

        return new GitMemoryDraft
        {
            Memory = memory,
            Snapshot = snapshot
        };
    }

    private static string ResolveProjectName(string repositoryPath)
    {
        return new DirectoryInfo(repositoryPath).Name;
    }

    private static string BuildTitleFromBranch(string branchName)
    {
        if (string.IsNullOrWhiteSpace(branchName))
        {
            return "Git task memory";
        }

        var normalized = branchName.Trim();

        var lastSlashIndex = normalized.LastIndexOf('/');

        if (lastSlashIndex >= 0 && lastSlashIndex < normalized.Length - 1)
        {
            normalized = normalized[(lastSlashIndex + 1)..];
        }

        normalized = normalized
            .Replace('_', ' ')
            .Replace('-', ' ')
            .Trim();

        while (normalized.Contains("  ", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("  ", " ", StringComparison.Ordinal);
        }

        return string.IsNullOrWhiteSpace(normalized)
            ? "Git task memory"
            : normalized;
    }
}
