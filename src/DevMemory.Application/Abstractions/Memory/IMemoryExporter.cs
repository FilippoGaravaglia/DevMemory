using DevMemory.Core;

namespace DevMemory.Application.Abstractions.Memory;

public interface IMemoryExporter
{
    string Export(TaskMemory memory);

    void Delete(TaskMemory memory);
}
