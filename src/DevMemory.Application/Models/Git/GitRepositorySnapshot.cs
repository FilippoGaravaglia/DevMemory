namespace DevMemory.Application.Models.Git;

public sealed class GitRepositorySnapshot
{
    public string RepositoryPath { get; init; } = string.Empty;

    public string BranchName { get; init; } = string.Empty;

    public string? LastCommitHash { get; init; }

    public string? LastCommitMessage { get; init; }

    public List<string> ChangedFiles { get; init; } = [];
}
