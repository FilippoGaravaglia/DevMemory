
using DevMemory.Application.Models.Git;

namespace DevMemory.Application.Abstractions.Git;

public interface IGitRepositoryInspector
{
    GitRepositorySnapshot Inspect(string repositoryPath);
}
