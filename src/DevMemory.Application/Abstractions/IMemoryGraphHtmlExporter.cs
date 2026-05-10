using DevMemory.Application.Models.Graph;

namespace DevMemory.Application.Abstractions;

public interface IMemoryGraphHtmlExporter
{
    string Export(MemoryGraph graph, string? outputPath = null);
}
