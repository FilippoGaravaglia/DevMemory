using DevMemory.Cli.Commands;

namespace DevMemory.Cli.CommandLine;

public sealed class CommandDispatcher
{
    private readonly IReadOnlyDictionary<string, ICommandHandler> _handlers;

    public CommandDispatcher(IEnumerable<ICommandHandler> handlers)
    {
        _handlers = handlers.ToDictionary(
            handler => handler.Name,
            StringComparer.OrdinalIgnoreCase);
    }

    public int Dispatch(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return CliExitCodes.InvalidCommand;
        }

        var command = args[0].Trim().ToLowerInvariant();

        if (command is "--help" or "-h")
        {
            command = "help";
        }

        try
        {
            if (!_handlers.TryGetValue(command, out var handler))
            {
                Console.Error.WriteLine($"Unknown command: {command}");
                Console.Error.WriteLine();
                PrintHelp();
                return CliExitCodes.InvalidCommand;
            }

            return handler.Execute(args);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return CliExitCodes.InvalidCommand;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Unexpected error.");
            Console.Error.WriteLine(ex.Message);
            return CliExitCodes.Failure;
        }
    }

    /// <summary>
    /// Prints the CLI help by delegating to the registered help command.
    /// </summary>
    private void PrintHelp()
    {
        if (_handlers.TryGetValue("help", out var helpHandler))
        {
            helpHandler.Execute([]);
        }
    }
}
