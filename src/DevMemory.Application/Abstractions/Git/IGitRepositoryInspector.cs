using DevMemory.Application.Models;

namespace DevMemory.Application.Abstractions;

public interface IGitRepositoryInspector
{
    GitRepositorySnapshot Inspect(string repositoryPath);
}
