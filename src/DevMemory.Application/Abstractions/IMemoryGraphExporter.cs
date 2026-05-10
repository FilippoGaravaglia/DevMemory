using DevMemory.Application.Models.Graph;

namespace DevMemory.Application.Abstractions;

public interface IMemoryGraphExporter
{
    string Export(MemoryGraph graph, string? outputPath = null);
}
