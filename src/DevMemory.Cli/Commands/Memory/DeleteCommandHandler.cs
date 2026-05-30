using DevMemory.Application;
using DevMemory.Application.Abstractions;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Cli.CommandLine;
using DevMemory.Infrastructure;
using DevMemory.Infrastructure.Ai.Factories;

namespace DevMemory.Cli.Commands.Memory;

/// <summary>
/// Deletes a memory from the primary local storage and derived indexes.
/// </summary>
public sealed class DeleteCommandHandler : ICommandHandler
{
    private readonly MemoryService _memoryService;
    private readonly Func<AiRuntimeOptions> _optionsFactory;
    private readonly Func<AiRuntimeOptions, IVectorMemoryStore?> _vectorMemoryStoreFactory;

    public DeleteCommandHandler(MemoryService memoryService)
        : this(
            memoryService,
            AiRuntimeOptionsProvider.GetOptions,
            VectorMemoryStoreFactory.Create)
    {
    }

    public DeleteCommandHandler(
        MemoryService memoryService,
        Func<AiRuntimeOptions> optionsFactory,
        Func<AiRuntimeOptions, IVectorMemoryStore?> vectorMemoryStoreFactory)
    {
        _memoryService = memoryService ?? throw new ArgumentNullException(nameof(memoryService));
        _optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
        _vectorMemoryStoreFactory = vectorMemoryStoreFactory
            ?? throw new ArgumentNullException(nameof(vectorMemoryStoreFactory));
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

        var vectorCleanupResult = TryDeleteVectorIndex(memoryId);

        Console.WriteLine("Memory deleted successfully.");
        Console.WriteLine();
        Console.WriteLine($"MemoryId: {result.DeletedMemory.Id:D}");
        Console.WriteLine($"Title: {result.DeletedMemory.Title}");
        Console.WriteLine();
        Console.WriteLine("Cleanup:");
        Console.WriteLine("  Primary JSON memory: deleted");
        Console.WriteLine("  Markdown export: deleted");
        PrintVectorCleanupResult(vectorCleanupResult);

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Attempts to delete the memory point from the configured vector store.
    /// </summary>
    private VectorCleanupResult TryDeleteVectorIndex(Guid memoryId)
    {
        try
        {
            var options = _optionsFactory();

            if (!options.VectorStore.IsEnabled)
            {
                return VectorCleanupResult.Skipped("Vector store is not configured.");
            }

            var vectorStore = _vectorMemoryStoreFactory(options);

            if (vectorStore is null)
            {
                return VectorCleanupResult.Skipped(
                    $"No vector store adapter is available for provider '{options.VectorStore.Provider}'.");
            }

            try
            {
                vectorStore
                    .DeleteAsync(memoryId, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();

                return VectorCleanupResult.Deleted();
            }
            finally
            {
                DisposeIfRequired(vectorStore);
            }
        }
        catch (Exception ex)
        {
            return VectorCleanupResult.Failed(ex.Message);
        }
    }

    /// <summary>
    /// Prints the vector index cleanup result.
    /// </summary>
    private static void PrintVectorCleanupResult(VectorCleanupResult result)
    {
        if (result.IsDeleted)
        {
            Console.WriteLine("  Vector index: deleted");

            return;
        }

        if (result.IsSkipped)
        {
            Console.WriteLine($"  Vector index: skipped ({result.Message})");

            return;
        }

        Console.WriteLine($"  Vector index: cleanup failed ({result.Message})");
        Console.WriteLine();
        Console.WriteLine("Warning:");
        Console.WriteLine("  The primary memory was deleted, but the derived vector index cleanup failed.");
        Console.WriteLine("  Run:");
        Console.WriteLine("    devmemory index --force");
        Console.WriteLine("  to rebuild the derived vector index.");
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

    /// <summary>
    /// Disposes an object when it implements IDisposable.
    /// </summary>
    private static void DisposeIfRequired(object? instance)
    {
        if (instance is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private sealed record VectorCleanupResult(
        bool IsDeleted,
        bool IsSkipped,
        string? Message)
    {
        public static VectorCleanupResult Deleted()
        {
            return new VectorCleanupResult(true, false, null);
        }

        public static VectorCleanupResult Skipped(string message)
        {
            return new VectorCleanupResult(false, true, message);
        }

        public static VectorCleanupResult Failed(string message)
        {
            return new VectorCleanupResult(false, false, message);
        }
    }
}
