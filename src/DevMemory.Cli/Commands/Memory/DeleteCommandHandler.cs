using DevMemory.Application;
using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Commands.Memory;

/// <summary>
/// Deletes a memory from the primary local storage.
/// </summary>
public sealed class DeleteCommandHandler : ICommandHandler
{
    private readonly MemoryService _memoryService;

    public DeleteCommandHandler(MemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    public string Name => "delete";

    public int Execute(string[] args)
    {
        if (args.Length < 2)
        {
            PrintUsageToError();

            return CliExitCodes.InvalidCommand;
        }

        if (!Guid.TryParse(args[1], out var memoryId))
        {
            Console.Error.WriteLine("Invalid memory id.");
            Console.Error.WriteLine("The memory id must be a valid GUID.");

            return CliExitCodes.InvalidCommand;
        }

        var skipConfirmation = args
            .Skip(2)
            .Any(argument => argument.Equals("--yes", StringComparison.OrdinalIgnoreCase));

        var existingMemory = _memoryService.GetById(memoryId);

        if (existingMemory is null)
        {
            Console.Error.WriteLine($"Memory not found: {memoryId}");

            return CliExitCodes.Failure;
        }

        if (!skipConfirmation && !ConfirmDeletion(existingMemory.Title, memoryId))
        {
            Console.WriteLine("Delete cancelled.");

            return CliExitCodes.Success;
        }

        var result = _memoryService.Delete(memoryId);

        if (!result.Success || result.DeletedMemory is null)
        {
            Console.Error.WriteLine(result.Error ?? $"Memory not found: {memoryId}");

            return CliExitCodes.Failure;
        }

        Console.WriteLine("Memory deleted successfully.");
        Console.WriteLine();
        Console.WriteLine($"MemoryId: {result.DeletedMemory.Id:D}");
        Console.WriteLine($"Title: {result.DeletedMemory.Title}");
        Console.WriteLine();
        Console.WriteLine("Note:");
        Console.WriteLine("  The primary JSON memory and Markdown export have been deleted.");
        Console.WriteLine("  If this memory was already indexed, run:");
        Console.WriteLine("    devmemory index --force");
        Console.WriteLine("  to rebuild the derived vector index.");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Asks the user to confirm memory deletion.
    /// </summary>
    private static bool ConfirmDeletion(string title, Guid memoryId)
    {
        Console.WriteLine("You are about to delete this memory:");
        Console.WriteLine();
        Console.WriteLine($"MemoryId: {memoryId:D}");
        Console.WriteLine($"Title: {title}");
        Console.WriteLine();
        Console.Write("Type 'yes' to confirm deletion: ");

        var confirmation = Console.ReadLine();

        return confirmation?.Equals("yes", StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Prints command usage to the error stream.
    /// </summary>
    private static void PrintUsageToError()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  devmemory delete <memory-id> [--yes]");
    }
}
