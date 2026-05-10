using DevMemory.Application;
using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Commands;

public sealed class ListCommandHandler : ICommandHandler
{
    private readonly MemoryService _memoryService;

    public ListCommandHandler(MemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    public string Name => "list";

    public int Execute(string[] args)
    {
        var memories = _memoryService.List();

        if (!memories.Any())
        {
            Console.WriteLine("No memories found.");
            return CliExitCodes.Success;
        }

        foreach (var memory in memories)
        {
            Console.WriteLine($"{memory.Id} | {memory.CreatedAt:u} | {memory.Project} | {memory.Area} | {memory.Title}");
        }

        return CliExitCodes.Success;
    }
}
