using DevMemory.Application;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Cli.CommandLine;
using DevMemory.Infrastructure;

namespace DevMemory.Cli.Commands.System;

/// <summary>
/// Runs general DevMemory health checks for local storage, configuration and runtime dependencies.
/// </summary>
public sealed class DoctorCommandHandler : ICommandHandler
{
    private readonly MemoryService _memoryService;
    private readonly Func<AiRuntimeOptions> _optionsFactory;
    private readonly Func<string> _configFilePathFactory;
    private readonly Func<string, bool> _fileExists;
    private readonly Func<string, bool> _directoryExists;
    private readonly Func<DoctorCheck> _gitCheckFactory;

    public DoctorCommandHandler(MemoryService memoryService)
        : this(
            memoryService,
            AiRuntimeOptionsProvider.GetOptions,
            () => new AiRuntimeConfigurationStore().ConfigFilePath,
            File.Exists,
            Directory.Exists,
            CheckGitAvailability)
    {
    }

    public DoctorCommandHandler(
        MemoryService memoryService,
        Func<AiRuntimeOptions> optionsFactory,
        Func<string> configFilePathFactory,
        Func<string, bool> fileExists,
        Func<string, bool> directoryExists,
        Func<DoctorCheck> gitCheckFactory)
    {
        _memoryService = memoryService ?? throw new ArgumentNullException(nameof(memoryService));
        _optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
        _configFilePathFactory = configFilePathFactory ?? throw new ArgumentNullException(nameof(configFilePathFactory));
        _fileExists = fileExists ?? throw new ArgumentNullException(nameof(fileExists));
        _directoryExists = directoryExists ?? throw new ArgumentNullException(nameof(directoryExists));
        _gitCheckFactory = gitCheckFactory ?? throw new ArgumentNullException(nameof(gitCheckFactory));
    }

    public string Name => "doctor";

    public int Execute(string[] args)
    {
        if (args.Length > 1)
        {
            PrintUsageToError();

            return CliExitCodes.InvalidCommand;
        }

        var checks = RunChecks();

        PrintResult(checks);

        return checks.Any(check => check.Status == DoctorCheckStatus.Fail)
            ? CliExitCodes.Failure
            : CliExitCodes.Success;
    }

    /// <summary>
    /// Runs all general DevMemory health checks.
    /// </summary>
    private List<DoctorCheck> RunChecks()
    {
        var checks = new List<DoctorCheck>
        {
            CheckStorage(),
            CheckMarkdownDirectory(),
            CheckConfigurationFile(),
            CheckAiRuntimeConfiguration(),
            _gitCheckFactory()
        };

        return checks;
    }

    /// <summary>
    /// Checks whether local JSON storage is readable.
    /// </summary>
    private DoctorCheck CheckStorage()
    {
        try
        {
            var memories = _memoryService.List();
            var storageFilePath = _memoryService.GetStorageFilePath();

            if (string.IsNullOrWhiteSpace(storageFilePath))
            {
                return DoctorCheck.Fail(
                    "Storage",
                    "Storage file path is empty.");
            }

            return DoctorCheck.Ok(
                "Storage",
                $"Readable. Memories: {memories.Count}. File: {storageFilePath}");
        }
        catch (Exception ex)
        {
            return DoctorCheck.Fail(
                "Storage",
                $"Storage is not readable. {ex.Message}");
        }
    }

    /// <summary>
    /// Checks whether the Markdown export directory exists.
    /// </summary>
    private DoctorCheck CheckMarkdownDirectory()
    {
        try
        {
            var markdownDirectoryPath = _memoryService.GetMarkdownDirectoryPath();

            if (string.IsNullOrWhiteSpace(markdownDirectoryPath))
            {
                return DoctorCheck.Fail(
                    "Markdown",
                    "Markdown directory path is empty.");
            }

            if (!_directoryExists(markdownDirectoryPath))
            {
                return DoctorCheck.Attention(
                    "Markdown",
                    $"Directory does not exist yet: {markdownDirectoryPath}");
            }

            return DoctorCheck.Ok(
                "Markdown",
                $"Directory exists: {markdownDirectoryPath}");
        }
        catch (Exception ex)
        {
            return DoctorCheck.Fail(
                "Markdown",
                $"Markdown directory check failed. {ex.Message}");
        }
    }

    /// <summary>
    /// Checks whether the persistent configuration file exists.
    /// </summary>
    private DoctorCheck CheckConfigurationFile()
    {
        try
        {
            var configFilePath = _configFilePathFactory();

            if (string.IsNullOrWhiteSpace(configFilePath))
            {
                return DoctorCheck.Fail(
                    "Configuration",
                    "Configuration file path is empty.");
            }

            if (!_fileExists(configFilePath))
            {
                return DoctorCheck.Attention(
                    "Configuration",
                    $"Persistent configuration file does not exist yet: {configFilePath}");
            }

            return DoctorCheck.Ok(
                "Configuration",
                $"Persistent configuration file exists: {configFilePath}");
        }
        catch (Exception ex)
        {
            return DoctorCheck.Fail(
                "Configuration",
                $"Configuration check failed. {ex.Message}");
        }
    }

    /// <summary>
    /// Checks whether AI/RAG runtime options are configured.
    /// </summary>
    private DoctorCheck CheckAiRuntimeConfiguration()
    {
        try
        {
            var options = _optionsFactory();

            if (options.IsFullRagEnabled)
            {
                return DoctorCheck.Ok(
                    "AI runtime",
                    $"Full RAG enabled. Chat: {options.Chat.Provider}. Embeddings: {options.Embedding.Provider}. Vector store: {options.VectorStore.Provider}.");
            }

            if (options.IsSemanticSearchEnabled)
            {
                return DoctorCheck.Attention(
                    "AI runtime",
                    $"Semantic search enabled, but chat is not fully configured. Embeddings: {options.Embedding.Provider}. Vector store: {options.VectorStore.Provider}.");
            }

            if (options.IsChatEnabled)
            {
                return DoctorCheck.Attention(
                    "AI runtime",
                    $"Chat enabled, but semantic search/RAG is not fully configured. Chat: {options.Chat.Provider}.");
            }

            return DoctorCheck.Attention(
                "AI runtime",
                "AI runtime is not configured. Use 'devmemory config set ...' or environment variables.");
        }
        catch (Exception ex)
        {
            return DoctorCheck.Fail(
                "AI runtime",
                $"AI runtime configuration check failed. {ex.Message}");
        }
    }

    /// <summary>
    /// Checks whether Git is available on the machine.
    /// </summary>
    private static DoctorCheck CheckGitAvailability()
    {
        try
        {
            using var process = new global::System.Diagnostics.Process();

            process.StartInfo = new global::System.Diagnostics.ProcessStartInfo
            {
                FileName = "git",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            process.Start();

            var output = process.StandardOutput.ReadToEnd().Trim();
            var error = process.StandardError.ReadToEnd().Trim();

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                return DoctorCheck.Ok(
                    "Git",
                    string.IsNullOrWhiteSpace(output)
                        ? "Git is available."
                        : output);
            }

            return DoctorCheck.Attention(
                "Git",
                string.IsNullOrWhiteSpace(error)
                    ? "Git command is available but returned a non-zero exit code."
                    : error);
        }
        catch (Exception ex)
        {
            return DoctorCheck.Attention(
                "Git",
                $"Git is not available or cannot be executed. {ex.Message}");
        }
    }

    /// <summary>
    /// Prints doctor command output.
    /// </summary>
    private static void PrintResult(List<DoctorCheck> checks)
    {
        Console.WriteLine("DevMemory doctor");
        Console.WriteLine("----------------");
        Console.WriteLine();

        foreach (var check in checks)
        {
            Console.WriteLine($"{FormatStatus(check.Status)} {check.Name}");
            Console.WriteLine($"   {check.Message}");
            Console.WriteLine();
        }

        var failedChecks = checks.Count(check => check.Status == DoctorCheckStatus.Fail);
        var attentionChecks = checks.Count(check => check.Status == DoctorCheckStatus.Attention);

        Console.WriteLine("Summary:");
        Console.WriteLine($"OK: {checks.Count(check => check.Status == DoctorCheckStatus.Ok)}");
        Console.WriteLine($"Attention: {attentionChecks}");
        Console.WriteLine($"Failed: {failedChecks}");
        Console.WriteLine();

        if (failedChecks > 0)
        {
            Console.WriteLine("Result: failed");
            return;
        }

        if (attentionChecks > 0)
        {
            Console.WriteLine("Result: attention required");
            Console.WriteLine();
            Console.WriteLine("Tip:");
            Console.WriteLine("  Run 'devmemory ai-doctor' for detailed Ollama/Qdrant diagnostics.");
            return;
        }

        Console.WriteLine("Result: ready");
    }

    /// <summary>
    /// Prints command usage to the error stream.
    /// </summary>
    private static void PrintUsageToError()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  devmemory doctor");
    }

    /// <summary>
    /// Formats a doctor check status for CLI output.
    /// </summary>
    private static string FormatStatus(DoctorCheckStatus status)
    {
        return status switch
        {
            DoctorCheckStatus.Ok => "[OK]",
            DoctorCheckStatus.Attention => "[ATTENTION]",
            DoctorCheckStatus.Fail => "[FAIL]",
            _ => "[UNKNOWN]"
        };
    }

    public sealed record DoctorCheck(
        string Name,
        DoctorCheckStatus Status,
        string Message)
    {
        public static DoctorCheck Ok(string name, string message)
        {
            return new DoctorCheck(name, DoctorCheckStatus.Ok, message);
        }

        public static DoctorCheck Attention(string name, string message)
        {
            return new DoctorCheck(name, DoctorCheckStatus.Attention, message);
        }

        public static DoctorCheck Fail(string name, string message)
        {
            return new DoctorCheck(name, DoctorCheckStatus.Fail, message);
        }
    }

    public enum DoctorCheckStatus
    {
        Ok,
        Attention,
        Fail
    }
}
