using DevMemory.Application.Abstractions;
using DevMemory.Application.Models;

namespace DevMemory.Application.Tests;

public sealed class GitMemoryDraftServiceTests
{
    [Fact]
    public void CreateDraft_WhenRepositorySnapshotIsAvailable_CreatesMemoryDraftFromGitContext()
    {
        // Arrange
        var inspector = new FakeGitRepositoryInspector
        {
            Snapshot = new GitRepositorySnapshot
            {
                RepositoryPath = "/Users/test/LogicalCommon",
                BranchName = "TGS-500_fix_revision_options",
                LastCommitHash = "abc1234",
                LastCommitMessage = "Fix revision options",
                ChangedFiles =
                [
                    "src/EstimateRevisionService.cs",
                    "tests/EstimateRevisionServiceTests.cs"
                ]
            }
        };

        var service = new GitMemoryDraftService(inspector);

        // Act
        var draft = service.CreateDraft("/Users/test/LogicalCommon");

        // Assert
        Assert.Equal("LogicalCommon", draft.Memory.Project);
        Assert.Equal("TGS-500_fix_revision_options", draft.Memory.Branch);
        Assert.Equal("TGS 500 fix revision options", draft.Memory.Title);
        Assert.Equal(
            [
                "src/EstimateRevisionService.cs",
                "tests/EstimateRevisionServiceTests.cs"
            ],
            draft.Memory.FilesTouched);
    }

    private sealed class FakeGitRepositoryInspector : IGitRepositoryInspector
    {
        public required GitRepositorySnapshot Snapshot { get; init; }

        public GitRepositorySnapshot Inspect(string repositoryPath)
        {
            return Snapshot;
        }
    }
}