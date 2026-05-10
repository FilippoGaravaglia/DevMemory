using DevMemory.Application;
using DevMemory.Cli.CommandLine;

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
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Search query is required.");
            Console.Error.WriteLine("Usage:");
            Console.Error.WriteLine("  dotnet run --project src/DevMemory.Cli -- search <query> [--project <project>] [--area <area>] [--tag <tag>]");
            return CliExitCodes.InvalidCommand;
        }

        var options = CommandOptions.BuildSearchOptions(args);

        if (string.IsNullOrWhiteSpace(options.Query))
        {
            Console.Error.WriteLine("Search query is required.");
            return CliExitCodes.InvalidCommand;
        }

        var results = _memoryService.Search(options);

        if (!results.Any())
        {
            Console.WriteLine("No matching memories found.");
            return CliExitCodes.Success;
        }

        foreach (var result in results)
        {
            var memory = result.Memory;
            Console.WriteLine($"{memory.Id} | score:{result.Score} | {memory.Project} | {memory.Area} | {memory.Title}");
        }

        return CliExitCodes.Success;
    }
}