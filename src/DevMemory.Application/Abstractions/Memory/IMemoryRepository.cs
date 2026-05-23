using DevMemory.Core;

namespace DevMemory.Application.Abstractions;

public interface IMemoryRepository
{
    List<TaskMemory> Load();

    void Save(List<TaskMemory> memories);

    string GetStorageFilePath();

    string GetMarkdownDirectoryPath();
}
