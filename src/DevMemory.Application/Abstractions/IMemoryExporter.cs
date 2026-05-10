using DevMemory.Core;

namespace DevMemory.Application.Abstractions;

public interface IMemoryExporter
{
    string Export(TaskMemory memory);
}
