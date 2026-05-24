using DevMemory.Application.Models.Graph;

namespace DevMemory.Application.Abstractions.Graph;

public interface IMemoryGraphHtmlExporter
{
    string Export(MemoryGraph graph, string? outputPath = null);
}
