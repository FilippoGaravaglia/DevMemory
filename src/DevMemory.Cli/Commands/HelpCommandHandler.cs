using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Commands;

public sealed class HelpCommandHandler : ICommandHandler
{
    public string Name => "help";

    public int Execute(string[] args)
    {
        Console.WriteLine("DevMemory - Local Developer Memory");
        Console.WriteLine("----------------------------------");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- add");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- list");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- search <query> [--project <project>] [--area <area>] [--tag <tag>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- show <memory-id>");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- storage");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- markdown");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- git-status [--path <repository-path>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- learn-from-git [--path <repository-path>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- graph-export [--output <file-path>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- graph-view [--output <file-path>]");
        Console.WriteLine();
        Console.WriteLine("Installed tool usage:");
        Console.WriteLine("  devmemory add");
        Console.WriteLine("  devmemory list");
        Console.WriteLine("  devmemory search <query> [--project <project>] [--area <area>] [--tag <tag>]");
        Console.WriteLine("  devmemory show <memory-id>");
        Console.WriteLine("  devmemory storage");
        Console.WriteLine("  devmemory markdown");
        Console.WriteLine("  devmemory git-status [--path <repository-path>]");
        Console.WriteLine("  devmemory learn-from-git [--path <repository-path>]");
        Console.WriteLine("  devmemory graph-export [--output <file-path>]");
        Console.WriteLine("  devmemory graph-view [--output <file-path>]");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  devmemory search revision");
        Console.WriteLine("  devmemory search revision --project LogicalCommon");
        Console.WriteLine("  devmemory search revision --area Estimate");
        Console.WriteLine("  devmemory search revision --tag dotnet");
        Console.WriteLine("  devmemory git-status");
        Console.WriteLine("  devmemory learn-from-git");
        Console.WriteLine("  devmemory graph-export");
        Console.WriteLine("  devmemory graph-view");
        Console.WriteLine();
        Console.WriteLine("Environment variables:");
        Console.WriteLine("  DEVMEMORY_HOME  Custom DevMemory storage directory");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine("  DEVMEMORY_HOME=~/devmemory-work devmemory storage");

        return CliExitCodes.Success;
    }
}