using DevMemory.Application;
using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Commands;

public sealed class GraphExportCommandHandler : ICommandHandler
{
    private readonly MemoryGraphService _memoryGraphService;

    public GraphExportCommandHandler(MemoryGraphService memoryGraphService)
    {
        _memoryGraphService = memoryGraphService;
    }

    public string Name => "graph-export";

    public int Execute(string[] args)
    {
        var outputPath = CommandOptions.ReadOutputOption(args);

        var result = _memoryGraphService.ExportGraph(outputPath);

        Console.WriteLine("Graph exported successfully.");
        Console.WriteLine($"Path: {result.FilePath}");
        Console.WriteLine($"Nodes: {result.NodesCount}");
        Console.WriteLine($"Edges: {result.EdgesCount}");

        return CliExitCodes.Success;
    }
}