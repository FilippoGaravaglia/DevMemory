using System.Globalization;
using DevMemory.Application;
using DevMemory.Application.Reports;
using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Commands.Memory;

public sealed class ReportCommandHandler : ICommandHandler
{
    private readonly MemoryService _memoryService;

    public ReportCommandHandler(MemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    public string Name => "report";

    /// <summary>
    /// Executes the report command.
    /// </summary>
    public int Execute(string[] args)
    {
        var request = ParseRequest(args);

        if (request.ShowHelp)
        {
            PrintUsage();
            return CliExitCodes.Success;
        }

        if (!string.IsNullOrWhiteSpace(request.Error))
        {
            Console.Error.WriteLine(request.Error);
            Console.Error.WriteLine();
            PrintUsageToError();

            return CliExitCodes.InvalidCommand;
        }

        var memories = _memoryService.List();
        var report = MemoryProjectReportService.BuildReport(memories, request.Project!);

        var outputPath = ResolveOutputPath(request.OutputPath, report.Project);

        if (File.Exists(outputPath) && !request.Force)
        {
            Console.Error.WriteLine($"Report file already exists: {outputPath}");
            Console.Error.WriteLine("Use --force to overwrite it.");

            return CliExitCodes.InvalidCommand;
        }

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(outputPath, report.MarkdownContent);

        Console.WriteLine("DevMemory project report");
        Console.WriteLine("------------------------");
        Console.WriteLine();
        Console.WriteLine($"Project: {report.Project}");
        Console.WriteLine($"Memories: {report.TotalMemories}");
        Console.WriteLine($"Areas: {report.Areas.Count}");
        Console.WriteLine($"Tags: {report.Tags.Count}");
        Console.WriteLine($"Files touched: {report.FilesTouched.Count}");
        Console.WriteLine($"Output: {outputPath}");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Parses command-line arguments.
    /// </summary>
    private static ReportCommandRequest ParseRequest(string[] args)
    {
        string? project = null;
        string? outputPath = null;
        var force = false;

        for (var i = 1; i < args.Length; i++)
        {
            var option = args[i];

            switch (option)
            {
                case "--help":
                case "-h":
                    return new ReportCommandRequest(
                        Project: null,
                        OutputPath: null,
                        Force: false,
                        ShowHelp: true,
                        Error: null);

                case "--project":
                    if (!TryReadOptionValue(args, ref i, option, out project, out var projectError))
                    {
                        return ReportCommandRequest.Invalid(projectError);
                    }

                    break;

                case "--output":
                    if (!TryReadOptionValue(args, ref i, option, out outputPath, out var outputError))
                    {
                        return ReportCommandRequest.Invalid(outputError);
                    }

                    break;

                case "--force":
                    force = true;
                    break;

                default:
                    return ReportCommandRequest.Invalid($"Unknown option for report command: {option}");
            }
        }

        if (string.IsNullOrWhiteSpace(project))
        {
            return ReportCommandRequest.Invalid("Missing required option: --project <project>");
        }

        return new ReportCommandRequest(
            Project: project,
            OutputPath: outputPath,
            Force: force,
            ShowHelp: false,
            Error: null);
    }

    /// <summary>
    /// Reads an option value from the current command-line position.
    /// </summary>
    private static bool TryReadOptionValue(
        string[] args,
        ref int index,
        string option,
        out string? value,
        out string? error)
    {
        if (index + 1 >= args.Length)
        {
            value = null;
            error = $"Missing value for option: {option}";
            return false;
        }

        var nextValue = args[index + 1];

        if (nextValue.StartsWith("--", StringComparison.Ordinal))
        {
            value = null;
            error = $"Missing value for option: {option}";
            return false;
        }

        value = nextValue;
        error = null;
        index++;

        return true;
    }

    /// <summary>
    /// Resolves the final report output path.
    /// </summary>
    private static string ResolveOutputPath(string? outputPath, string project)
    {
        if (!string.IsNullOrWhiteSpace(outputPath))
        {
            return Path.GetFullPath(outputPath);
        }

        var devMemoryHome = Environment.GetEnvironmentVariable("DEVMEMORY_HOME");

        var homeDirectory = string.IsNullOrWhiteSpace(devMemoryHome)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".devmemory")
            : devMemoryHome;

        var reportsDirectory = Path.Combine(homeDirectory, "reports");
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var safeProject = ToSafeFileName(project);

        return Path.Combine(reportsDirectory, $"{safeProject}-report-{timestamp}.md");
    }

    /// <summary>
    /// Converts a project name into a safe file name segment.
    /// </summary>
    private static string ToSafeFileName(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var cleanCharacters = value
            .Trim()
            .Select(character => invalidCharacters.Contains(character) ? '-' : character)
            .ToArray();

        var result = new string(cleanCharacters)
            .Replace(' ', '-')
            .ToLowerInvariant();

        return string.IsNullOrWhiteSpace(result)
            ? "project"
            : result;
    }

    /// <summary>
    /// Prints command usage.
    /// </summary>
    private static void PrintUsage()
    {
        Console.WriteLine("DevMemory report");
        Console.WriteLine("----------------");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  devmemory report --project <project>");
        Console.WriteLine("  devmemory report --project <project> --output <file-path>");
        Console.WriteLine("  devmemory report --project <project> --output <file-path> --force");
        Console.WriteLine();
        Console.WriteLine("Description:");
        Console.WriteLine("  Generates a Markdown report from local memories for a specific project.");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --project  Project name to include in the report.");
        Console.WriteLine("  --output   Optional Markdown output path.");
        Console.WriteLine("  --force    Overwrite the output file when it already exists.");
        Console.WriteLine("  --help     Show report command help.");
        Console.WriteLine();
        Console.WriteLine("Notes:");
        Console.WriteLine("  This command does not require AI, Ollama or Qdrant.");
        Console.WriteLine("  It reads local JSON storage and writes a Markdown report.");
    }

    /// <summary>
    /// Prints command usage to stderr.
    /// </summary>
    private static void PrintUsageToError()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  devmemory report --project <project>");
        Console.Error.WriteLine("  devmemory report --project <project> --output <file-path>");
        Console.Error.WriteLine("  devmemory report --project <project> --output <file-path> --force");
    }

    private sealed record ReportCommandRequest(
        string? Project,
        string? OutputPath,
        bool Force,
        bool ShowHelp,
        string? Error)
    {
        public static ReportCommandRequest Invalid(string? error)
        {
            return new ReportCommandRequest(
                Project: null,
                OutputPath: null,
                Force: false,
                ShowHelp: false,
                Error: error);
        }
    }
}
