using DevMemory.Application;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Presentation;

namespace DevMemory.Cli.Commands;

public sealed class SearchCommandHandler : ICommandHandler
{
    private readonly MemoryService _memoryService;

    public SearchCommandHandler(MemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    public string Name => "search";

    public int Execute(string[] args)
    {
        var options = CommandOptions.BuildSearchOptions(args);

        if (string.IsNullOrWhiteSpace(options.Query) &&
            string.IsNullOrWhiteSpace(options.Project) &&
            string.IsNullOrWhiteSpace(options.Area) &&
            string.IsNullOrWhiteSpace(options.Tag))
        {
            Console.Error.WriteLine("Search requires a query or at least one filter.");
            Console.Error.WriteLine("Example:");
            Console.Error.WriteLine("  devmemory search revision --project LogicalCommon");

            return CliExitCodes.InvalidCommand;
        }

        var results = _memoryService.Search(options);

        MemoryConsolePrinter.PrintSearchResults(results);

        return CliExitCodes.Success;
    }
}
