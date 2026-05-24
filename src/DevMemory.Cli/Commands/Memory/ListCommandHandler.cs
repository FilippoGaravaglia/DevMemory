using DevMemory.Application;
using DevMemory.Application.Models;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Presentation;

namespace DevMemory.Cli.Commands.Memory;

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
