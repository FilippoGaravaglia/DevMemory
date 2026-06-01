using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Commands.System;

public sealed class SetupCommandHandler : ICommandHandler
{
    public string Name => "setup";

    public int Execute(string[] args)
    {
        if (args.Length > 2)
        {
            PrintUsageToError();

            return CliExitCodes.InvalidCommand;
        }

        if (args.Length == 1)
        {
            PrintGeneralSetup();

            return CliExitCodes.Success;
        }

        var option = args[1];

        return option switch
        {
            "--local-ai" => PrintLocalAiSetup(),
            "--demo" => PrintDemoSetup(),
            "--check" => PrintCheckSetup(),
            "--help" or "-h" => PrintHelp(),
            _ => PrintInvalidOption(option)
        };
    }

    /// <summary>
    /// Prints the default first-run setup guide.
    /// </summary>
    private static void PrintGeneralSetup()
    {
        Console.WriteLine("DevMemory setup");
        Console.WriteLine("---------------");
        Console.WriteLine();
        Console.WriteLine("This guide helps you configure DevMemory for local usage.");
        Console.WriteLine();
        Console.WriteLine("Recommended first steps:");
        Console.WriteLine();
        Console.WriteLine("1. Verify the installed CLI version:");
        Console.WriteLine("   devmemory version");
        Console.WriteLine();
        Console.WriteLine("2. Check the local environment:");
        Console.WriteLine("   devmemory doctor");
        Console.WriteLine();
        Console.WriteLine("3. Create your first memory:");
        Console.WriteLine("   devmemory add");
        Console.WriteLine();
        Console.WriteLine("4. Search and inspect memories:");
        Console.WriteLine("   devmemory list");
        Console.WriteLine("   devmemory search \"your topic\"");
        Console.WriteLine("   devmemory show <memory-id>");
        Console.WriteLine();
        Console.WriteLine("5. Try the isolated local demo without touching your real data:");
        Console.WriteLine("   ./scripts/demo-local.sh");
        Console.WriteLine();
        Console.WriteLine("Optional setup modes:");
        Console.WriteLine("   devmemory setup --local-ai");
        Console.WriteLine("   devmemory setup --demo");
        Console.WriteLine("   devmemory setup --check");
        Console.WriteLine();
        Console.WriteLine("Documentation:");
        Console.WriteLine("   README.md");
        Console.WriteLine("   docs/demo.md");
    }

    /// <summary>
    /// Prints the local AI setup guide.
    /// </summary>
    private static int PrintLocalAiSetup()
    {
        Console.WriteLine("DevMemory local AI setup");
        Console.WriteLine("------------------------");
        Console.WriteLine();
        Console.WriteLine("DevMemory can use Ollama and Qdrant for semantic search, related memories and RAG.");
        Console.WriteLine();
        Console.WriteLine("1. Pull local Ollama models:");
        Console.WriteLine("   ./scripts/dev-ai-local.sh pull-models");
        Console.WriteLine();
        Console.WriteLine("2. Start local AI services:");
        Console.WriteLine("   ./scripts/dev-ai-local.sh start");
        Console.WriteLine();
        Console.WriteLine("3. Diagnose local AI runtime:");
        Console.WriteLine("   ./scripts/dev-ai-local.sh doctor");
        Console.WriteLine("   devmemory ai-doctor");
        Console.WriteLine();
        Console.WriteLine("4. Persist DevMemory AI configuration:");
        Console.WriteLine("   devmemory config set chat-provider ollama");
        Console.WriteLine("   devmemory config set embedding-provider ollama");
        Console.WriteLine("   devmemory config set vector-store qdrant");
        Console.WriteLine("   devmemory config set ollama-chat-model llama3.2");
        Console.WriteLine("   devmemory config set ollama-embedding-model nomic-embed-text");
        Console.WriteLine("   devmemory config set qdrant-collection devmemory_memories");
        Console.WriteLine();
        Console.WriteLine("5. Index memories and query them:");
        Console.WriteLine("   devmemory index");
        Console.WriteLine("   devmemory semantic-search \"local AI runtime\"");
        Console.WriteLine("   devmemory related <memory-id>");
        Console.WriteLine("   devmemory ask --rag \"How did we handle this?\"");
        Console.WriteLine();
        Console.WriteLine("Configuration precedence:");
        Console.WriteLine("   Environment variables > ~/.devmemory/config.json > default values");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Prints the isolated demo setup guide.
    /// </summary>
    private static int PrintDemoSetup()
    {
        Console.WriteLine("DevMemory isolated demo");
        Console.WriteLine("-----------------------");
        Console.WriteLine();
        Console.WriteLine("Run the isolated local demo without touching your real ~/.devmemory data:");
        Console.WriteLine();
        Console.WriteLine("   ./scripts/demo-local.sh");
        Console.WriteLine();
        Console.WriteLine("Keep generated demo data for inspection:");
        Console.WriteLine();
        Console.WriteLine("   DEVMEMORY_KEEP_DEMO_HOME=true ./scripts/demo-local.sh");
        Console.WriteLine();
        Console.WriteLine("The demo shows:");
        Console.WriteLine("   devmemory doctor");
        Console.WriteLine("   devmemory list");
        Console.WriteLine("   devmemory search");
        Console.WriteLine("   devmemory show");
        Console.WriteLine("   devmemory timeline");
        Console.WriteLine("   devmemory edit");
        Console.WriteLine("   devmemory graph-export");
        Console.WriteLine("   devmemory graph-view");
        Console.WriteLine("   optional semantic search, related memories and RAG");
        Console.WriteLine();
        Console.WriteLine("Full guide:");
        Console.WriteLine("   docs/demo.md");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Prints commands that can be used to check the current setup.
    /// </summary>
    private static int PrintCheckSetup()
    {
        Console.WriteLine("DevMemory setup checks");
        Console.WriteLine("----------------------");
        Console.WriteLine();
        Console.WriteLine("Run these commands to verify your local setup:");
        Console.WriteLine();
        Console.WriteLine("   devmemory version");
        Console.WriteLine("   devmemory doctor");
        Console.WriteLine("   devmemory ai-status");
        Console.WriteLine("   devmemory ai-doctor");
        Console.WriteLine("   devmemory config show");
        Console.WriteLine();
        Console.WriteLine("For full release validation from the repository root:");
        Console.WriteLine();
        Console.WriteLine("   ./scripts/release-check.sh");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Prints command help.
    /// </summary>
    private static int PrintHelp()
    {
        PrintUsage();

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Prints an invalid option error.
    /// </summary>
    private static int PrintInvalidOption(string option)
    {
        Console.Error.WriteLine($"Unknown setup option: {option}");
        Console.Error.WriteLine();

        PrintUsageToError();

        return CliExitCodes.InvalidCommand;
    }

    /// <summary>
    /// Prints usage to standard output.
    /// </summary>
    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  devmemory setup");
        Console.WriteLine("  devmemory setup --local-ai");
        Console.WriteLine("  devmemory setup --demo");
        Console.WriteLine("  devmemory setup --check");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --local-ai   Show local Ollama/Qdrant setup steps.");
        Console.WriteLine("  --demo       Show isolated local demo instructions.");
        Console.WriteLine("  --check      Show setup validation commands.");
        Console.WriteLine("  --help, -h   Show this help message.");
    }

    /// <summary>
    /// Prints usage to standard error.
    /// </summary>
    private static void PrintUsageToError()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  devmemory setup");
        Console.Error.WriteLine("  devmemory setup --local-ai");
        Console.Error.WriteLine("  devmemory setup --demo");
        Console.Error.WriteLine("  devmemory setup --check");
    }
}
