using DevMemory.Application;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Presentation;

namespace DevMemory.Cli.Commands;

public sealed class ShowCommandHandler : ICommandHandler
{
    private readonly MemoryService _memoryService;

    public ShowCommandHandler(MemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    public string Name => "show";

    public int Execute(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Memory id is required.");
            Console.Error.WriteLine("Usage:");
            Console.Error.WriteLine("  dotnet run --project src/DevMemory.Cli -- show <memory-id>");
            return CliExitCodes.InvalidCommand;
        }

        if (!Guid.TryParse(args[1], out var id))
        {
            Console.Error.WriteLine("Invalid memory id.");
            return CliExitCodes.InvalidCommand;
        }

        var memory = _memoryService.GetById(id);

        if (memory is null)
        {
            Console.Error.WriteLine("Memory not found.");
            return CliExitCodes.Failure;
        }

        MemoryConsolePrinter.PrintMemoryDetails(memory);

        return CliExitCodes.Success;
    }
}