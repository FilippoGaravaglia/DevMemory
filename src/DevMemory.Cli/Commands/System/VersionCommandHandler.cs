using System.Reflection;
using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Commands.System;

public sealed class VersionCommandHandler : ICommandHandler
{
    private readonly TextWriter _output;

    public VersionCommandHandler()
        : this(Console.Out)
    {
    }

    public VersionCommandHandler(TextWriter output)
    {
        _output = output;
    }

    public string Name => "version";

    public int Execute(string[] args)
    {
        var version = GetApplicationVersion();

        _output.WriteLine($"DevMemory {version}");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Gets the current application version from assembly metadata.
    /// </summary>
    private static string GetApplicationVersion()
    {
        var assembly = typeof(VersionCommandHandler).Assembly;

        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            return informationalVersion.Split('+')[0];
        }

        return assembly.GetName().Version?.ToString(3) ?? "unknown";
    }
}
