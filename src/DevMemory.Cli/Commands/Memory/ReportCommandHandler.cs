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

        var report = MemoryProjectReportService.BuildReport(
            memories,
            new MemoryProjectReportOptions(
                Project: request.Project!,
                Area: request.Area,
                Tag: request.Tag,
                From: request.From,
                To: request.To));

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
        Console.WriteLine($"Area filter: {report.Area ?? "-"}");
        Console.WriteLine($"Tag filter: {report.Tag ?? "-"}");
        Console.WriteLine($"From: {FormatDate(report.From)}");
        Console.WriteLine($"To: {FormatDate(report.To)}");
        Console.WriteLine($"Memories: {report.TotalMemories}");
        Console.WriteLine($"Areas: {report.Areas.Count}");
        Console.WriteLine($"Tags: {report.Tags.Count}");
        Console.WriteLine($"Files touched: {report.FilesTouched.Count}");
        Console.WriteLine($"Output: {outputPath}");

        return CliExitCodes.Success;
    }

    #region Helpers

    /// <summary>
    /// Parses command-line arguments.
    /// </summary>
    private static ReportCommandRequest ParseRequest(string[] args)
    {
        string? project = null;
        string? area = null;
        string? tag = null;
        string? outputPath = null;
        DateTime? from = null;
        DateTime? to = null;
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
                        Area: null,
                        Tag: null,
                        From: null,
                        To: null,
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

                case "--area":
                    if (!TryReadOptionValue(args, ref i, option, out area, out var areaError))
                    {
                        return ReportCommandRequest.Invalid(areaError);
                    }

                    break;

                case "--tag":
                    if (!TryReadOptionValue(args, ref i, option, out tag, out var tagError))
                    {
                        return ReportCommandRequest.Invalid(tagError);
                    }

                    break;

                case "--from":
                    if (!TryReadDateOptionValue(args, ref i, option, out from, out var fromError))
                    {
                        return ReportCommandRequest.Invalid(fromError);
                    }

                    break;

                case "--to":
                    if (!TryReadDateOptionValue(args, ref i, option, out to, out var toError))
                    {
                        return ReportCommandRequest.Invalid(toError);
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

        if (from is not null && to is not null && from.Value.Date > to.Value.Date)
        {
            return ReportCommandRequest.Invalid("--from cannot be greater than --to.");
        }

        return new ReportCommandRequest(
            Project: project,
            Area: area,
            Tag: tag,
            From: from,
            To: to,
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
    /// Reads and parses a yyyy-MM-dd date option value from the current command-line position.
    /// </summary>
    private static bool TryReadDateOptionValue(
        string[] args,
        ref int index,
        string option,
        out DateTime? value,
        out string? error)
    {
        if (!TryReadOptionValue(args, ref index, option, out var rawValue, out error))
        {
            value = null;
            return false;
        }

        if (!DateTime.TryParseExact(
                rawValue,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedValue))
        {
            value = null;
            error = $"Invalid value for option {option}: expected yyyy-MM-dd.";
            return false;
        }

        value = parsedValue.Date;
        error = null;
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
    /// Formats a nullable date for CLI output.
    /// </summary>
    private static string FormatDate(DateTime? value)
    {
        return value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "-";
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
        Console.WriteLine("  devmemory report --project <project> --area <area>");
        Console.WriteLine("  devmemory report --project <project> --tag <tag>");
        Console.WriteLine("  devmemory report --project <project> --from <yyyy-MM-dd> --to <yyyy-MM-dd>");
        Console.WriteLine("  devmemory report --project <project> --output <file-path>");
        Console.WriteLine("  devmemory report --project <project> --output <file-path> --force");
        Console.WriteLine();
        Console.WriteLine("Description:");
        Console.WriteLine("  Generates a Markdown report from local memories for a specific project.");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --project  Project name to include in the report.");
        Console.WriteLine("  --area     Optional area filter.");
        Console.WriteLine("  --tag      Optional tag filter.");
        Console.WriteLine("  --from     Optional start date filter, format yyyy-MM-dd.");
        Console.WriteLine("  --to       Optional end date filter, format yyyy-MM-dd.");
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
        Console.Error.WriteLine("  devmemory report --project <project> --area <area>");
        Console.Error.WriteLine("  devmemory report --project <project> --tag <tag>");
        Console.Error.WriteLine("  devmemory report --project <project> --from <yyyy-MM-dd> --to <yyyy-MM-dd>");
        Console.Error.WriteLine("  devmemory report --project <project> --output <file-path>");
        Console.Error.WriteLine("  devmemory report --project <project> --output <file-path> --force");
    }

    private sealed record ReportCommandRequest(
        string? Project,
        string? Area,
        string? Tag,
        DateTime? From,
        DateTime? To,
        string? OutputPath,
        bool Force,
        bool ShowHelp,
        string? Error)
    {
        public static ReportCommandRequest Invalid(string? error)
        {
            return new ReportCommandRequest(
                Project: null,
                Area: null,
                Tag: null,
                From: null,
                To: null,
                OutputPath: null,
                Force: false,
                ShowHelp: false,
                Error: error);
        }
    }

    #endregion
}
