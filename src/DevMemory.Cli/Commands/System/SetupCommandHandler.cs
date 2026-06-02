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
            "--wizard" => RunSetupWizard(),
            "--next" => PrintNextSteps(),
            "--checklist" => PrintSetupChecklist(),
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
        Console.WriteLine("   devmemory setup --wizard");
        Console.WriteLine("   devmemory setup --next");
        Console.WriteLine("   devmemory setup --checklist");
        Console.WriteLine("   devmemory setup --local-ai");
        Console.WriteLine("   devmemory setup --demo");
        Console.WriteLine("   devmemory setup --check");
        Console.WriteLine();
        Console.WriteLine("Documentation:");
        Console.WriteLine("   README.md");
        Console.WriteLine("   docs/demo.md");
    }

    /// <summary>
    /// Runs a safe interactive first-run setup wizard.
    /// </summary>
    private static int RunSetupWizard()
    {
        Console.WriteLine("DevMemory interactive setup wizard");
        Console.WriteLine("----------------------------------");
        Console.WriteLine();
        Console.WriteLine("This wizard helps you understand the recommended first-run setup.");
        Console.WriteLine();
        Console.WriteLine("No files will be modified.");
        Console.WriteLine("No configuration will be written.");
        Console.WriteLine("No external services will be started.");
        Console.WriteLine();

        Console.WriteLine("Step 1 - Check the installed CLI");
        Console.WriteLine("Run:");
        Console.WriteLine("   devmemory --version");
        Console.WriteLine("   devmemory help");
        Console.WriteLine();

        Console.WriteLine("Step 2 - Inspect local storage");
        Console.WriteLine("Run:");
        Console.WriteLine("   devmemory storage");
        Console.WriteLine();

        Console.WriteLine("Step 3 - Run diagnostics");
        Console.WriteLine("Run:");
        Console.WriteLine("   devmemory doctor");
        Console.WriteLine();

        Console.WriteLine("Step 4 - Create and inspect your first memory");
        Console.WriteLine("Run:");
        Console.WriteLine("   devmemory add");
        Console.WriteLine("   devmemory list");
        Console.WriteLine("   devmemory show <memory-id>");
        Console.WriteLine();

        Console.WriteLine("Step 5 - Use local memory features without AI");
        Console.WriteLine("Run:");
        Console.WriteLine("   devmemory search \"your topic\"");
        Console.WriteLine("   devmemory timeline");
        Console.WriteLine("   devmemory graph-export");
        Console.WriteLine("   devmemory graph-view");
        Console.WriteLine();

        Console.Write("Do you want to see the optional local AI/RAG setup steps? [y/N]: ");
        var answer = Console.ReadLine();

        if (IsYes(answer))
        {
            Console.WriteLine();
            Console.WriteLine("Optional local AI/RAG setup");
            Console.WriteLine("---------------------------");
            Console.WriteLine();
            Console.WriteLine("Prerequisites:");
            Console.WriteLine("   - Docker Desktop running");
            Console.WriteLine("   - Ollama installed and running");
            Console.WriteLine();
            Console.WriteLine("Pull local models:");
            Console.WriteLine("   ./scripts/dev-ai-local.sh pull-models");
            Console.WriteLine();
            Console.WriteLine("Start local AI services:");
            Console.WriteLine("   ./scripts/dev-ai-local.sh start");
            Console.WriteLine();
            Console.WriteLine("Diagnose local AI runtime:");
            Console.WriteLine("   ./scripts/dev-ai-local.sh doctor");
            Console.WriteLine("   devmemory ai-doctor");
            Console.WriteLine();
            Console.WriteLine("Configure DevMemory:");
            Console.WriteLine("   devmemory config set chat-provider ollama");
            Console.WriteLine("   devmemory config set embedding-provider ollama");
            Console.WriteLine("   devmemory config set vector-store qdrant");
            Console.WriteLine("   devmemory config set ollama-chat-model llama3.2");
            Console.WriteLine("   devmemory config set ollama-embedding-model nomic-embed-text");
            Console.WriteLine("   devmemory config set qdrant-collection devmemory_memories");
            Console.WriteLine();
            Console.WriteLine("Index and query:");
            Console.WriteLine("   devmemory index");
            Console.WriteLine("   devmemory semantic-search \"your topic\"");
            Console.WriteLine("   devmemory related <memory-id>");
            Console.WriteLine("   devmemory ask --rag --show-context \"your question\"");
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("Local AI/RAG setup skipped.");
            Console.WriteLine("You can review it later with:");
            Console.WriteLine("   devmemory setup --local-ai");
        }

        Console.WriteLine();
        Console.WriteLine("Recommended next command:");
        Console.WriteLine("   devmemory doctor");
        Console.WriteLine();
        Console.WriteLine("For the full checklist:");
        Console.WriteLine("   devmemory setup --checklist");
        Console.WriteLine();
        Console.WriteLine("Setup wizard completed.");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Returns true when a console answer should be interpreted as yes.
    /// </summary>
    private static bool IsYes(string? value)
    {
        return string.Equals(value?.Trim(), "y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value?.Trim(), "yes", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Prints a first-run checklist that helps users complete the initial DevMemory setup.
    /// </summary>
    private static int PrintSetupChecklist()
    {
        Console.WriteLine("DevMemory first-run checklist");
        Console.WriteLine("-----------------------------");
        Console.WriteLine();
        Console.WriteLine("Use this checklist to validate a new local DevMemory setup.");
        Console.WriteLine();
        Console.WriteLine("[ ] 1. Verify the installed CLI version");
        Console.WriteLine("       devmemory version");
        Console.WriteLine();
        Console.WriteLine("[ ] 2. Inspect the local storage path");
        Console.WriteLine("       devmemory storage");
        Console.WriteLine();
        Console.WriteLine("[ ] 3. Run the general health check");
        Console.WriteLine("       devmemory doctor");
        Console.WriteLine();
        Console.WriteLine("[ ] 4. Create your first structured memory");
        Console.WriteLine("       devmemory add");
        Console.WriteLine();
        Console.WriteLine("[ ] 5. List and inspect saved memories");
        Console.WriteLine("       devmemory list");
        Console.WriteLine("       devmemory show <memory-id>");
        Console.WriteLine();
        Console.WriteLine("[ ] 6. Search local memories without AI");
        Console.WriteLine("       devmemory search \"your topic\"");
        Console.WriteLine();
        Console.WriteLine("[ ] 7. Explore the project timeline");
        Console.WriteLine("       devmemory timeline");
        Console.WriteLine();
        Console.WriteLine("[ ] 8. Generate the local knowledge graph");
        Console.WriteLine("       devmemory graph-export");
        Console.WriteLine("       devmemory graph-view");
        Console.WriteLine();
        Console.WriteLine("[ ] 9. Optional: run the isolated demo");
        Console.WriteLine("       ./scripts/demo-local.sh");
        Console.WriteLine();
        Console.WriteLine("[ ] 10. Optional: configure local AI/RAG");
        Console.WriteLine("        devmemory setup --local-ai");
        Console.WriteLine("        devmemory index");
        Console.WriteLine("        devmemory semantic-search \"your topic\"");
        Console.WriteLine("        devmemory ask --rag \"your question\"");
        Console.WriteLine();
        Console.WriteLine("Checklist principles:");
        Console.WriteLine("   - Core memory features work without AI.");
        Console.WriteLine("   - JSON local storage is the source of truth.");
        Console.WriteLine("   - Markdown, graph and vector data are derived artifacts.");
        Console.WriteLine("   - Local AI/RAG is optional and can be configured later.");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Prints recommended next steps for a new DevMemory user.
    /// </summary>
    private static int PrintNextSteps()
    {
        Console.WriteLine("DevMemory recommended next steps");
        Console.WriteLine("--------------------------------");
        Console.WriteLine();
        Console.WriteLine("Use this guide when you are not sure what to do next.");
        Console.WriteLine();
        Console.WriteLine("1. Check your local environment:");
        Console.WriteLine("   devmemory doctor");
        Console.WriteLine();
        Console.WriteLine("2. Inspect where DevMemory stores local data:");
        Console.WriteLine("   devmemory storage");
        Console.WriteLine();
        Console.WriteLine("3. Create your first structured memory:");
        Console.WriteLine("   devmemory add");
        Console.WriteLine();
        Console.WriteLine("4. List and inspect saved memories:");
        Console.WriteLine("   devmemory list");
        Console.WriteLine("   devmemory show <memory-id>");
        Console.WriteLine();
        Console.WriteLine("5. Search local memories without AI:");
        Console.WriteLine("   devmemory search \"your topic\"");
        Console.WriteLine();
        Console.WriteLine("6. Explore project evolution:");
        Console.WriteLine("   devmemory timeline");
        Console.WriteLine();
        Console.WriteLine("7. Generate a local graph view:");
        Console.WriteLine("   devmemory graph-export");
        Console.WriteLine("   devmemory graph-view");
        Console.WriteLine();
        Console.WriteLine("8. Try the isolated demo:");
        Console.WriteLine("   ./scripts/demo-local.sh");
        Console.WriteLine();
        Console.WriteLine("9. Optional: enable local AI/RAG:");
        Console.WriteLine("   devmemory setup --local-ai");
        Console.WriteLine();
        Console.WriteLine("For the full first-run checklist:");
        Console.WriteLine("   devmemory setup --checklist");

        return CliExitCodes.Success;
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
        Console.WriteLine("  devmemory setup --wizard");
        Console.WriteLine("  devmemory setup --next");
        Console.WriteLine("  devmemory setup --checklist");
        Console.WriteLine("  devmemory setup --local-ai");
        Console.WriteLine("  devmemory setup --demo");
        Console.WriteLine("  devmemory setup --check");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --wizard    Run a safe interactive first-run setup wizard.");
        Console.WriteLine("  --next       Show recommended next steps.");
        Console.WriteLine("  --checklist  Show a first-run setup checklist.");
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
        Console.Error.WriteLine("  devmemory setup --wizard");
        Console.Error.WriteLine("  devmemory setup --next");
        Console.Error.WriteLine("  devmemory setup --checklist");
        Console.Error.WriteLine("  devmemory setup --local-ai");
        Console.Error.WriteLine("  devmemory setup --demo");
        Console.Error.WriteLine("  devmemory setup --check");
    }
}
