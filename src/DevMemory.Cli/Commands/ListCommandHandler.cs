using DevMemory.Application;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Presentation;

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

        MemoryConsolePrinter.PrintMemoryList(memories);

        return CliExitCodes.Success;
    }
}
