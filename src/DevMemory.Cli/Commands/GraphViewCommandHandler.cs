using DevMemory.Application;
using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Commands;

public sealed class GraphViewCommandHandler : ICommandHandler
{
    private readonly MemoryGraphService _memoryGraphService;

    public GraphViewCommandHandler(MemoryGraphService memoryGraphService)
    {
        _memoryGraphService = memoryGraphService;
    }

    public string Name => "graph-view";

    public int Execute(string[] args)
    {
        var outputPath = CommandOptions.ReadOutputOption(args);

        var result = _memoryGraphService.ExportGraphView(outputPath);

        Console.WriteLine("Graph view generated successfully.");
        Console.WriteLine($"Path: {result.FilePath}");
        Console.WriteLine($"Nodes: {result.NodesCount}");
        Console.WriteLine($"Edges: {result.EdgesCount}");

        return CliExitCodes.Success;
    }
}