using System.Diagnostics;
using DevMemory.Application.Abstractions;
using DevMemory.Application.Models;
using DevMemory.Application.Filtering;

namespace DevMemory.Infrastructure.Git;

public sealed class GitRepositoryInspector : IGitRepositoryInspector
{
    public GitRepositorySnapshot Inspect(string repositoryPath)
    {
        if (string.IsNullOrWhiteSpace(repositoryPath))
        {
            repositoryPath = Directory.GetCurrentDirectory();
        }

        var fullPath = Path.GetFullPath(repositoryPath);

        EnsureGitRepository(fullPath);

        var branchName = RunGit(fullPath, "rev-parse --abbrev-ref HEAD").Trim();

        var lastCommitHash = RunGit(fullPath, "rev-parse --short HEAD").Trim();
        var lastCommitMessage = RunGit(fullPath, "log -1 --pretty=%s").Trim();

        var changedFilesOutput = RunGit(fullPath, "status --porcelain");
        var changedFiles = ParseChangedFiles(changedFilesOutput);

        return new GitRepositorySnapshot
        {
            RepositoryPath = fullPath,
            BranchName = branchName,
            LastCommitHash = string.IsNullOrWhiteSpace(lastCommitHash) ? null : lastCommitHash,
            LastCommitMessage = string.IsNullOrWhiteSpace(lastCommitMessage) ? null : lastCommitMessage,
            ChangedFiles = changedFiles
        };
    }

    private static void EnsureGitRepository(string repositoryPath)
    {
        var result = RunProcess(repositoryPath, "git", "rev-parse --is-inside-work-tree");

        if (result.ExitCode != 0 || !result.StandardOutput.Trim().Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"The path is not a Git repository: {repositoryPath}");
        }
    }

    private static string RunGit(string workingDirectory, string arguments)
    {
        var result = RunProcess(workingDirectory, "git", arguments);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(result.StandardError);
        }

        return result.StandardOutput;
    }

    private static ProcessResult RunProcess(string workingDirectory, string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo);

        if (process is null)
        {
            throw new InvalidOperationException($"Unable to start process: {fileName}");
        }

        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();

        process.WaitForExit();

        return new ProcessResult(process.ExitCode, standardOutput, standardError);
    }

    private static List<string> ParseChangedFiles(string statusOutput)
    {
        if (string.IsNullOrWhiteSpace(statusOutput))
        {
            return [];
        }
    
        var changedFiles = statusOutput
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParseStatusLine)
            .Where(file => !string.IsNullOrWhiteSpace(file));
    
        return MemoryFileFilter.Filter(changedFiles);
    }

    private static string ParseStatusLine(string line)
    {
        if (line.Length <= 3)
        {
            return string.Empty;
        }

        var filePath = line[3..].Trim();

        var renameSeparatorIndex = filePath.IndexOf(" -> ", StringComparison.Ordinal);

        return renameSeparatorIndex >= 0
            ? filePath[(renameSeparatorIndex + 4)..].Trim()
            : filePath;
    }

    private sealed record ProcessResult(
        int ExitCode,
        string StandardOutput,
        string StandardError);
}