using DevMemory.Application;
using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Commands;

public sealed class MarkdownCommandHandler : ICommandHandler
{
    private readonly MemoryService _memoryService;

    public MarkdownCommandHandler(MemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    public string Name => "markdown";

    public int Execute(string[] args)
    {
        Console.WriteLine(_memoryService.GetMarkdownDirectoryPath());
        return CliExitCodes.Success;
    }
}
