using System.Globalization;
using DevMemory.Application;
using DevMemory.Cli.CommandLine;
using DevMemory.Core;

namespace DevMemory.Cli.Commands.Memory;

/// <summary>
/// Prints saved memories as a chronological timeline.
/// </summary>
public sealed class TimelineCommandHandler : ICommandHandler
{
    private const int DefaultLimit = 25;

    private readonly MemoryService _memoryService;

    public TimelineCommandHandler(MemoryService memoryService)
    {
        _memoryService = memoryService ?? throw new ArgumentNullException(nameof(memoryService));
    }

    public string Name => "timeline";

    public int Execute(string[] args)
    {
        TimelineCommandRequest request;

        try
        {
            request = ParseRequest(args);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine();

            PrintUsageToError();

            return CliExitCodes.InvalidCommand;
        }

        var memories = ApplyLimit(
                ApplyFilters(_memoryService.List(), request),
                request.Limit)
            .OrderByDescending(memory => memory.CreatedAt)
            .ToList();

        PrintTimeline(memories, request);

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Parses timeline command-line arguments.
    /// </summary>
    private static TimelineCommandRequest ParseRequest(string[] args)
    {
        string? project = null;
        string? area = null;
        string? tag = null;
        var limit = DefaultLimit;

        for (var index = 1; index < args.Length; index++)
        {
            var value = args[index];

            if (value.Equals("--project", StringComparison.OrdinalIgnoreCase))
            {
                project = CommandOptions.ReadOptionValue(args, ref index, "--project");
                continue;
            }

            if (value.Equals("--area", StringComparison.OrdinalIgnoreCase))
            {
                area = CommandOptions.ReadOptionValue(args, ref index, "--area");
                continue;
            }

            if (value.Equals("--tag", StringComparison.OrdinalIgnoreCase))
            {
                tag = CommandOptions.ReadOptionValue(args, ref index, "--tag");
                continue;
            }

            if (value.Equals("--limit", StringComparison.OrdinalIgnoreCase))
            {
                var limitValue = CommandOptions.ReadOptionValue(args, ref index, "--limit");

                if (!int.TryParse(limitValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out limit) ||
                    limit <= 0)
                {
                    throw new ArgumentException("Option --limit must be a positive integer.");
                }

                continue;
            }

            throw new ArgumentException($"Unknown timeline option: {value}");
        }

        return new TimelineCommandRequest(
            project,
            area,
            tag,
            limit);
    }

    /// <summary>
    /// Applies optional filters to timeline memories.
    /// </summary>
    private static List<TaskMemory> ApplyFilters(
        IReadOnlyCollection<TaskMemory> memories,
        TimelineCommandRequest request)
    {
        IEnumerable<TaskMemory> filteredMemories = memories;

        if (!string.IsNullOrWhiteSpace(request.Project))
        {
            filteredMemories = filteredMemories.Where(memory =>
                memory.Project.Equals(request.Project.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.Area))
        {
            filteredMemories = filteredMemories.Where(memory =>
                memory.Area.Equals(request.Area.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            filteredMemories = filteredMemories.Where(memory =>
                memory.Tags.Any(tag =>
                    tag.Equals(request.Tag.Trim(), StringComparison.OrdinalIgnoreCase)));
        }

        return filteredMemories.ToList();
    }

    /// <summary>
    /// Applies the requested maximum number of timeline entries.
    /// </summary>
    private static List<TaskMemory> ApplyLimit(
        IReadOnlyCollection<TaskMemory> memories,
        int limit)
    {
        return memories
            .OrderByDescending(memory => memory.CreatedAt)
            .Take(limit)
            .ToList();
    }

    /// <summary>
    /// Prints timeline output grouped by creation date.
    /// </summary>
    private static void PrintTimeline(
        List<TaskMemory> memories,
        TimelineCommandRequest request)
    {
        Console.WriteLine("DevMemory timeline");
        Console.WriteLine("------------------");
        Console.WriteLine();
        Console.WriteLine("Filters:");
        Console.WriteLine($"Project: {FormatOptional(request.Project)}");
        Console.WriteLine($"Area: {FormatOptional(request.Area)}");
        Console.WriteLine($"Tag: {FormatOptional(request.Tag)}");
        Console.WriteLine($"Limit: {request.Limit.ToString(CultureInfo.InvariantCulture)}");
        Console.WriteLine();
        Console.WriteLine($"Results: {memories.Count}");
        Console.WriteLine();

        if (memories.Count == 0)
        {
            Console.WriteLine("No memories found for the selected timeline filters.");
            Console.WriteLine();
            Console.WriteLine("Create a memory with:");
            Console.WriteLine("  devmemory add");

            return;
        }

        foreach (var group in memories.GroupBy(memory => memory.CreatedAt.Date))
        {
            Console.WriteLine(group.Key.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            foreach (var memory in group.OrderByDescending(memory => memory.CreatedAt))
            {
                PrintTimelineEntry(memory);
            }

            Console.WriteLine();
        }
    }

    /// <summary>
    /// Prints a single timeline entry.
    /// </summary>
    private static void PrintTimelineEntry(TaskMemory memory)
    {
        Console.WriteLine($"  {memory.CreatedAt:HH:mm}  {memory.Title}");
        Console.WriteLine($"         MemoryId: {memory.Id:D}");
        Console.WriteLine($"         Project: {FormatOptional(memory.Project)}");
        Console.WriteLine($"         Area: {FormatOptional(memory.Area)}");
        Console.WriteLine($"         Tags: {FormatCollection(memory.Tags)}");
    }

    /// <summary>
    /// Prints command usage to the error stream.
    /// </summary>
    private static void PrintUsageToError()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  devmemory timeline [--project <project>] [--area <area>] [--tag <tag>] [--limit <number>]");
    }

    /// <summary>
    /// Formats an optional string value for CLI output.
    /// </summary>
    private static string FormatOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    /// <summary>
    /// Formats a collection value for CLI output.
    /// </summary>
    private static string FormatCollection(List<string> values)
    {
        return values.Count == 0
            ? "-"
            : string.Join(", ", values);
    }

    private sealed record TimelineCommandRequest(
        string? Project,
        string? Area,
        string? Tag,
        int Limit);
}
