using DevMemory.Application;
using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Commands;

public sealed class StorageCommandHandler : ICommandHandler
{
    private readonly MemoryService _memoryService;

    public StorageCommandHandler(MemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    public string Name => "storage";

    public int Execute(string[] args)
    {
        Console.WriteLine(_memoryService.GetStorageFilePath());
        return CliExitCodes.Success;
    }
}