using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Commands.System;

public sealed class HelpCommandHandler : ICommandHandler
{
    public string Name => "help";

    /// <summary>
    /// Executes the help command.
    /// </summary>
    public int Execute(string[] args)
    {
        if (args.Length <= 1)
        {
            PrintHelp();
            return CliExitCodes.Success;
        }

        var commandName = args[1];

        return commandName switch
        {
            "setup" => PrintSetupHelp(),
            "config" => PrintConfigHelp(),
            "memory" => PrintMemoryHelp(),
            "ask" => PrintAskHelp(),
            "index" => PrintIndexHelp(),
            "--help" or "-h" => PrintHelpAndReturnSuccess(),
            _ => PrintUnknownHelpTopic(commandName)
        };
    }

    /// <summary>
    /// Prints the general help output and returns a successful exit code.
    /// </summary>
    private static int PrintHelpAndReturnSuccess()
    {
        PrintHelp();

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Prints the main DevMemory help output.
    /// </summary>
    private static void PrintHelp()
    {
        Console.WriteLine("DevMemory - Local Developer Memory");
        Console.WriteLine("----------------------------------");
        Console.WriteLine();

        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- add");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- list");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- search <query> [--project <project>] [--area <area>] [--tag <tag>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- show <memory-id>");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- edit <memory-id> [options]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- delete <memory-id> [--yes]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- timeline [--project <project>] [--area <area>] [--tag <tag>] [--limit <number>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- insights");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- report --project <project> [--output <file-path>] [--force]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- storage");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- markdown");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- git-status [--path <repository-path>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- learn-from-git [--path <repository-path>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- graph-export [--output <file-path>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- graph-view [--output <file-path>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- ai-status");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- ai-doctor");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- doctor");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- ask <question>");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- ask --rag <question> [--show-context] [--limit <number>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- index [--dry-run] [--force] [--limit <number>] [--project <project>] [--area <area>] [--tag <tag>] [--show-text]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- semantic-search <query> [--limit <number>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- related <memory-id> [--limit <number>] [--show-preview]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- config show");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- config set <key> <value>");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- config reset");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- version");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- --version");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- setup [--wizard|--next|--checklist|--local-ai|--demo|--check]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- help [command]");
        Console.WriteLine();

        Console.WriteLine("Installed tool usage:");
        Console.WriteLine("  devmemory add");
        Console.WriteLine("  devmemory list");
        Console.WriteLine("  devmemory search <query> [--project <project>] [--area <area>] [--tag <tag>]");
        Console.WriteLine("  devmemory show <memory-id>");
        Console.WriteLine("  devmemory edit <memory-id> [options]");
        Console.WriteLine("  devmemory delete <memory-id> [--yes]");
        Console.WriteLine("  devmemory timeline [--project <project>] [--area <area>] [--tag <tag>] [--limit <number>]");
        Console.WriteLine("  devmemory insights");
        Console.WriteLine("  devmemory report --project <project> [--output <file-path>] [--force]");
        Console.WriteLine("  devmemory storage");
        Console.WriteLine("  devmemory markdown");
        Console.WriteLine("  devmemory git-status [--path <repository-path>]");
        Console.WriteLine("  devmemory learn-from-git [--path <repository-path>]");
        Console.WriteLine("  devmemory graph-export [--output <file-path>]");
        Console.WriteLine("  devmemory graph-view [--output <file-path>]");
        Console.WriteLine("  devmemory ai-status");
        Console.WriteLine("  devmemory ai-doctor");
        Console.WriteLine("  devmemory doctor");
        Console.WriteLine("  devmemory ask <question>");
        Console.WriteLine("  devmemory ask --rag <question> [--show-context] [--limit <number>]");
        Console.WriteLine("  devmemory index [--dry-run] [--force] [--limit <number>] [--project <project>] [--area <area>] [--tag <tag>] [--show-text]");
        Console.WriteLine("  devmemory semantic-search <query> [--limit <number>]");
        Console.WriteLine("  devmemory related <memory-id> [--limit <number>] [--show-preview]");
        Console.WriteLine("  devmemory config show");
        Console.WriteLine("  devmemory config set <key> <value>");
        Console.WriteLine("  devmemory config reset");
        Console.WriteLine("  devmemory version");
        Console.WriteLine("  devmemory --version");
        Console.WriteLine("  devmemory -v");
        Console.WriteLine("  devmemory setup [--wizard|--next|--checklist|--local-ai|--demo|--check]");
        Console.WriteLine("  devmemory help [command]");
        Console.WriteLine();

        Console.WriteLine("Commands:");
        Console.WriteLine("  add              Create a new structured memory.");
        Console.WriteLine("  list             List saved memories.");
        Console.WriteLine("  search           Search memories by text and optional filters.");
        Console.WriteLine("  show             Show a memory by id.");
        Console.WriteLine("  edit             Edit an existing memory by id.");
        Console.WriteLine("  delete           Delete a memory by id from local storage.");
        Console.WriteLine("  timeline         Show saved memories as a chronological timeline.");
        Console.WriteLine("  insights         Show aggregated memory statistics and suggestions.");
        Console.WriteLine("  report           Generate a Markdown report for a project.");
        Console.WriteLine("  storage          Show the current storage file path.");
        Console.WriteLine("  markdown         Show the Markdown export directory.");
        Console.WriteLine("  git-status       Inspect the current or selected Git repository.");
        Console.WriteLine("  learn-from-git   Create a memory draft from Git context.");
        Console.WriteLine("  graph-export     Export the memory graph as JSON.");
        Console.WriteLine("  graph-view       Generate the local HTML graph view.");
        Console.WriteLine("  ai-status        Show the current AI/RAG runtime configuration status.");
        Console.WriteLine("  ai-doctor        Diagnose the local AI runtime configuration.");
        Console.WriteLine("  doctor           Run general DevMemory health checks.");
        Console.WriteLine("  ask              Ask a question using the configured AI chat provider.");
        Console.WriteLine("  index            Index local memories into the configured vector store.");
        Console.WriteLine("  semantic-search  Search indexed memories using semantic similarity.");
        Console.WriteLine("  related          Find indexed memories semantically related to a memory.");
        Console.WriteLine("  config           Show, set or reset persistent DevMemory configuration.");
        Console.WriteLine("  version          Show the current DevMemory version.");
        Console.WriteLine("  help             Show this help message.");
        Console.WriteLine("  setup            Show first-run setup guidance.");
        Console.WriteLine();

        Console.WriteLine("Global aliases:");
        Console.WriteLine("  --help, -h       Show this help message.");
        Console.WriteLine("  --version, -v    Show the current DevMemory version.");
        Console.WriteLine();

        Console.WriteLine("Examples:");
        Console.WriteLine("  devmemory search revision");
        Console.WriteLine("  devmemory search revision --project LogicalCommon");
        Console.WriteLine("  devmemory search revision --area Estimate");
        Console.WriteLine("  devmemory search revision --tag dotnet");
        Console.WriteLine("  devmemory edit <memory-id> --title \"Updated title\"");
        Console.WriteLine("  devmemory edit <memory-id> --add-tag rag");
        Console.WriteLine("  devmemory edit <memory-id> --remove-tag test");
        Console.WriteLine("  devmemory edit <memory-id> --solution \"Updated implementation notes\"");
        Console.WriteLine("  devmemory delete <memory-id>");
        Console.WriteLine("  devmemory delete <memory-id> --yes");
        Console.WriteLine("  devmemory timeline");
        Console.WriteLine("  devmemory timeline --project DevMemory");
        Console.WriteLine("  devmemory timeline --area AI");
        Console.WriteLine("  devmemory timeline --tag rag");
        Console.WriteLine("  devmemory timeline --project DevMemory --limit 10");
        Console.WriteLine("  devmemory insights");
        Console.WriteLine("  devmemory report --project DevMemory");
        Console.WriteLine("  devmemory report --project DevMemory --output ./devmemory-report.md");
        Console.WriteLine("  devmemory report --project DevMemory --output ./devmemory-report.md --force");
        Console.WriteLine("  devmemory git-status");
        Console.WriteLine("  devmemory learn-from-git");
        Console.WriteLine("  devmemory graph-export");
        Console.WriteLine("  devmemory graph-view");
        Console.WriteLine("  devmemory ai-status");
        Console.WriteLine("  devmemory ai-doctor");
        Console.WriteLine("  devmemory doctor");
        Console.WriteLine("  devmemory config show");
        Console.WriteLine("  devmemory config set chat-provider ollama");
        Console.WriteLine("  devmemory config set embedding-provider ollama");
        Console.WriteLine("  devmemory config set vector-store qdrant");
        Console.WriteLine("  devmemory config set ollama-chat-model llama3.2");
        Console.WriteLine("  devmemory config set ollama-embedding-model nomic-embed-text");
        Console.WriteLine("  devmemory config set qdrant-collection devmemory_memories");
        Console.WriteLine("  devmemory ask \"What did I change last time in this area?\"");
        Console.WriteLine("  devmemory ask --rag \"How did we handle estimate revision cloning?\"");
        Console.WriteLine("  devmemory ask --rag --show-context \"How did we handle estimate revision cloning?\"");
        Console.WriteLine("  devmemory ask --rag \"What did I change in MongoDB mapping?\" --limit 3");
        Console.WriteLine("  devmemory index");
        Console.WriteLine("  devmemory index --dry-run");
        Console.WriteLine("  devmemory index --dry-run --show-text --limit 1");
        Console.WriteLine("  devmemory index --force");
        Console.WriteLine("  devmemory index --limit 3");
        Console.WriteLine("  devmemory index --force --limit 3");
        Console.WriteLine("  devmemory index --project LogicalCommon");
        Console.WriteLine("  devmemory index --area Estimate");
        Console.WriteLine("  devmemory index --tag mongodb");
        Console.WriteLine("  devmemory index --project LogicalCommon --area Estimate --limit 3");
        Console.WriteLine("  devmemory semantic-search \"estimate revision cloning\"");
        Console.WriteLine("  devmemory semantic-search \"mongodb mapping issue\" --limit 3");
        Console.WriteLine("  devmemory related <memory-id>");
        Console.WriteLine("  devmemory related <memory-id> --limit 3");
        Console.WriteLine("  devmemory related <memory-id> --show-preview");
        Console.WriteLine("  devmemory version");
        Console.WriteLine("  devmemory --version");
        Console.WriteLine("  devmemory setup");
        Console.WriteLine("  devmemory setup --wizard");
        Console.WriteLine("  devmemory setup --next");
        Console.WriteLine("  devmemory setup --checklist");
        Console.WriteLine("  devmemory setup --local-ai");
        Console.WriteLine("  devmemory setup --demo");
        Console.WriteLine("  devmemory setup --check");
        Console.WriteLine("  devmemory help setup");
        Console.WriteLine("  devmemory help config");
        Console.WriteLine("  devmemory help memory");
        Console.WriteLine("  devmemory help ask");
        Console.WriteLine("  devmemory help index");
        Console.WriteLine();

        Console.WriteLine("Environment variables:");
        Console.WriteLine("  DEVMEMORY_HOME                       Custom DevMemory storage directory");
        Console.WriteLine("  DEVMEMORY_CHAT_PROVIDER              Chat provider: none, ollama, openai, gemini, anthropic");
        Console.WriteLine("  DEVMEMORY_EMBEDDING_PROVIDER         Embedding provider: none, ollama, openai, gemini");
        Console.WriteLine("  DEVMEMORY_VECTOR_STORE               Vector store: none, qdrant");
        Console.WriteLine("  DEVMEMORY_OLLAMA_ENDPOINT            Ollama endpoint");
        Console.WriteLine("  DEVMEMORY_OLLAMA_EMBEDDING_MODEL     Ollama embedding model");
        Console.WriteLine("  DEVMEMORY_OLLAMA_CHAT_MODEL          Ollama chat model");
        Console.WriteLine("  DEVMEMORY_QDRANT_ENDPOINT            Qdrant endpoint");
        Console.WriteLine("  DEVMEMORY_QDRANT_COLLECTION          Qdrant collection name");
        Console.WriteLine();

        Console.WriteLine("Persistent configuration:");
        Console.WriteLine("  devmemory config show");
        Console.WriteLine("  devmemory config set chat-provider ollama");
        Console.WriteLine("  devmemory config set embedding-provider ollama");
        Console.WriteLine("  devmemory config set vector-store qdrant");
        Console.WriteLine("  devmemory config reset");
        Console.WriteLine();

        Console.WriteLine("Configuration precedence:");
        Console.WriteLine("  Environment variables > ~/.devmemory/config.json > default values");
        Console.WriteLine();

        Console.WriteLine("Environment examples:");
        Console.WriteLine("  DEVMEMORY_HOME=~/devmemory-work devmemory storage");
        Console.WriteLine("  DEVMEMORY_CHAT_PROVIDER=ollama devmemory ai-status");
        Console.WriteLine("  DEVMEMORY_CHAT_PROVIDER=ollama devmemory ask \"What did I change last time?\"");
        Console.WriteLine("  DEVMEMORY_CHAT_PROVIDER=ollama DEVMEMORY_EMBEDDING_PROVIDER=ollama DEVMEMORY_VECTOR_STORE=qdrant devmemory ask --rag \"How did we handle estimate revisions?\"");
        Console.WriteLine("  DEVMEMORY_EMBEDDING_PROVIDER=ollama DEVMEMORY_VECTOR_STORE=qdrant devmemory index");
        Console.WriteLine("  DEVMEMORY_EMBEDDING_PROVIDER=ollama DEVMEMORY_VECTOR_STORE=qdrant devmemory semantic-search \"estimate revision\"");
    }

    /// <summary>
    /// Prints command-specific help for the setup command.
    /// </summary>
    private static int PrintSetupHelp()
    {
        Console.WriteLine("DevMemory setup");
        Console.WriteLine("---------------");
        Console.WriteLine();

        Console.WriteLine("Usage:");
        Console.WriteLine("  devmemory setup");
        Console.WriteLine("  devmemory setup --wizard");
        Console.WriteLine("  devmemory setup --next");
        Console.WriteLine("  devmemory setup --checklist");
        Console.WriteLine("  devmemory setup --local-ai");
        Console.WriteLine("  devmemory setup --demo");
        Console.WriteLine("  devmemory setup --check");
        Console.WriteLine("  devmemory setup --help");
        Console.WriteLine();

        Console.WriteLine("Options:");
        Console.WriteLine("  --wizard     Run a safe interactive first-run setup wizard.");
        Console.WriteLine("  --next       Show recommended next steps.");
        Console.WriteLine("  --checklist  Show a first-run setup checklist.");
        Console.WriteLine("  --local-ai   Show local Ollama/Qdrant setup steps.");
        Console.WriteLine("  --demo       Show isolated local demo instructions.");
        Console.WriteLine("  --check      Show setup validation commands.");
        Console.WriteLine("  --help, -h   Show setup command help.");
        Console.WriteLine();

        Console.WriteLine("Notes:");
        Console.WriteLine("  The setup command is safe by default.");
        Console.WriteLine("  It does not modify local data unless a future explicit write option is added.");
        Console.WriteLine("  The setup wizard does not write configuration and does not start external services.");
        Console.WriteLine();

        Console.WriteLine("Examples:");
        Console.WriteLine("  devmemory setup");
        Console.WriteLine("  devmemory setup --wizard");
        Console.WriteLine("  devmemory setup --next");
        Console.WriteLine("  devmemory setup --checklist");
        Console.WriteLine("  devmemory setup --local-ai");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Prints command-specific help for the config command.
    /// </summary>
    private static int PrintConfigHelp()
    {
        Console.WriteLine("DevMemory config");
        Console.WriteLine("----------------");
        Console.WriteLine();

        Console.WriteLine("Usage:");
        Console.WriteLine("  devmemory config show");
        Console.WriteLine("  devmemory config set <key> <value>");
        Console.WriteLine("  devmemory config reset");
        Console.WriteLine();

        Console.WriteLine("Description:");
        Console.WriteLine("  Manages persistent local DevMemory configuration.");
        Console.WriteLine("  Configuration is stored locally under the DevMemory home directory.");
        Console.WriteLine();

        Console.WriteLine("Supported keys:");
        Console.WriteLine("  chat-provider             Chat provider: none, ollama, openai, gemini, anthropic");
        Console.WriteLine("  embedding-provider        Embedding provider: none, ollama, openai, gemini");
        Console.WriteLine("  vector-store              Vector store: none, qdrant");
        Console.WriteLine("  ollama-endpoint           Ollama endpoint");
        Console.WriteLine("  ollama-chat-model         Ollama chat model");
        Console.WriteLine("  ollama-embedding-model    Ollama embedding model");
        Console.WriteLine("  qdrant-endpoint           Qdrant endpoint");
        Console.WriteLine("  qdrant-collection         Qdrant collection name");
        Console.WriteLine();

        Console.WriteLine("Configuration precedence:");
        Console.WriteLine("  Environment variables > ~/.devmemory/config.json > default values");
        Console.WriteLine();

        Console.WriteLine("Local Ollama/Qdrant example:");
        Console.WriteLine("  devmemory config set chat-provider ollama");
        Console.WriteLine("  devmemory config set embedding-provider ollama");
        Console.WriteLine("  devmemory config set vector-store qdrant");
        Console.WriteLine("  devmemory config set ollama-chat-model llama3.2");
        Console.WriteLine("  devmemory config set ollama-embedding-model nomic-embed-text");
        Console.WriteLine("  devmemory config set qdrant-collection devmemory_memories");
        Console.WriteLine();

        Console.WriteLine("Useful related commands:");
        Console.WriteLine("  devmemory config show");
        Console.WriteLine("  devmemory ai-status");
        Console.WriteLine("  devmemory ai-doctor");
        Console.WriteLine("  devmemory index");
        Console.WriteLine("  devmemory semantic-search \"your topic\"");
        Console.WriteLine("  devmemory ask --rag \"your question\"");
        Console.WriteLine();

        Console.WriteLine("Notes:");
        Console.WriteLine("  Core memory commands do not require AI configuration.");
        Console.WriteLine("  Semantic search, related memories and RAG require embedding/vector configuration.");
        Console.WriteLine("  RAG also requires a configured chat provider.");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Prints command-specific help for the memory lifecycle commands.
    /// </summary>
    private static int PrintMemoryHelp()
    {
        Console.WriteLine("DevMemory memory lifecycle");
        Console.WriteLine("--------------------------");
        Console.WriteLine();

        Console.WriteLine("Usage:");
        Console.WriteLine("  devmemory add");
        Console.WriteLine("  devmemory list");
        Console.WriteLine("  devmemory show <memory-id>");
        Console.WriteLine("  devmemory search <query> [--project <project>] [--area <area>] [--tag <tag>]");
        Console.WriteLine("  devmemory edit <memory-id> [options]");
        Console.WriteLine("  devmemory delete <memory-id> [--yes]");
        Console.WriteLine("  devmemory timeline [--project <project>] [--area <area>] [--tag <tag>] [--limit <number>]");
        Console.WriteLine("  devmemory insights");
        Console.WriteLine("  devmemory report --project <project> [--output <file-path>] [--force]");
        Console.WriteLine("  devmemory storage");
        Console.WriteLine("  devmemory markdown");
        Console.WriteLine();

        Console.WriteLine("Description:");
        Console.WriteLine("  Manages local structured developer memories.");
        Console.WriteLine("  The local JSON storage file is the source of truth.");
        Console.WriteLine("  Markdown exports and vector entries are derived artifacts.");
        Console.WriteLine();

        Console.WriteLine("Core workflow:");
        Console.WriteLine("  devmemory add");
        Console.WriteLine("  devmemory list");
        Console.WriteLine("  devmemory search \"your topic\"");
        Console.WriteLine("  devmemory show <memory-id>");
        Console.WriteLine("  devmemory insights");
        Console.WriteLine("  devmemory report --project <project>");
        Console.WriteLine();

        Console.WriteLine("Search examples:");
        Console.WriteLine("  devmemory search \"revision\"");
        Console.WriteLine("  devmemory search \"mongodb mapping\" --project LogicalCommon");
        Console.WriteLine("  devmemory search \"estimate\" --area Estimate");
        Console.WriteLine("  devmemory search \"qdrant\" --tag ai");
        Console.WriteLine();

        Console.WriteLine("Edit examples:");
        Console.WriteLine("  devmemory edit <memory-id> --title \"Updated title\"");
        Console.WriteLine("  devmemory edit <memory-id> --solution \"Updated implementation notes\"");
        Console.WriteLine("  devmemory edit <memory-id> --add-tag rag");
        Console.WriteLine("  devmemory edit <memory-id> --remove-tag test");
        Console.WriteLine("  devmemory edit <memory-id> --add-file src/Example.cs");
        Console.WriteLine("  devmemory edit <memory-id> --add-test ExampleTests");
        Console.WriteLine();

        Console.WriteLine("Delete examples:");
        Console.WriteLine("  devmemory delete <memory-id>");
        Console.WriteLine("  devmemory delete <memory-id> --yes");
        Console.WriteLine();

        Console.WriteLine("Timeline examples:");
        Console.WriteLine("  devmemory timeline");
        Console.WriteLine("  devmemory timeline --project DevMemory");
        Console.WriteLine("  devmemory timeline --area AI");
        Console.WriteLine("  devmemory timeline --tag rag");
        Console.WriteLine("  devmemory timeline --limit 10");
        Console.WriteLine();

        Console.WriteLine("Notes:");
        Console.WriteLine("  Core memory commands do not require AI, Ollama or Qdrant.");
        Console.WriteLine("  Classic search reads directly from local JSON storage.");
        Console.WriteLine("  Editing regenerates the derived Markdown export.");
        Console.WriteLine("  If a memory was indexed into Qdrant, rebuild the vector index after editing.");
        Console.WriteLine("  Insights help identify projects, areas, tags and follow-up actions.");
        Console.WriteLine("  Reports generate Markdown summaries for project handover, review and documentation.");
        Console.WriteLine();

        Console.WriteLine("Useful related commands:");
        Console.WriteLine("  devmemory help index");
        Console.WriteLine("  devmemory semantic-search \"your topic\"");
        Console.WriteLine("  devmemory related <memory-id>");
        Console.WriteLine("  devmemory ask --rag \"your question\"");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Prints command-specific help for the ask command.
    /// </summary>
    private static int PrintAskHelp()
    {
        Console.WriteLine("DevMemory ask");
        Console.WriteLine("-------------");
        Console.WriteLine();

        Console.WriteLine("Usage:");
        Console.WriteLine("  devmemory ask <question>");
        Console.WriteLine("  devmemory ask --rag <question>");
        Console.WriteLine("  devmemory ask --rag --show-context <question>");
        Console.WriteLine("  devmemory ask --rag <question> --limit <number>");
        Console.WriteLine();

        Console.WriteLine("Description:");
        Console.WriteLine("  Asks a question using the configured AI chat provider.");
        Console.WriteLine("  With --rag, DevMemory retrieves relevant indexed memories first and uses them as context.");
        Console.WriteLine();

        Console.WriteLine("Options:");
        Console.WriteLine("  --rag           Enable retrieval-augmented generation using indexed memories.");
        Console.WriteLine("  --show-context  Print the retrieved memories used as context.");
        Console.WriteLine("  --limit         Maximum number of retrieved memories to use as context.");
        Console.WriteLine();

        Console.WriteLine("Examples:");
        Console.WriteLine("  devmemory ask \"Reply with only: hello\"");
        Console.WriteLine("  devmemory ask --rag \"How did we validate the local AI runtime?\"");
        Console.WriteLine("  devmemory ask --rag --show-context \"What did I decide about Qdrant?\"");
        Console.WriteLine("  devmemory ask --rag \"What did I change in MongoDB mapping?\" --limit 3");
        Console.WriteLine();

        Console.WriteLine("Requirements:");
        Console.WriteLine("  Basic ask requires a configured chat provider.");
        Console.WriteLine("  RAG requires a chat provider, an embedding provider, a vector store and indexed memories.");
        Console.WriteLine();

        Console.WriteLine("Typical local configuration:");
        Console.WriteLine("  devmemory config set chat-provider ollama");
        Console.WriteLine("  devmemory config set embedding-provider ollama");
        Console.WriteLine("  devmemory config set vector-store qdrant");
        Console.WriteLine();

        Console.WriteLine("Useful related commands:");
        Console.WriteLine("  devmemory help config");
        Console.WriteLine("  devmemory help index");
        Console.WriteLine("  devmemory ai-status");
        Console.WriteLine("  devmemory ai-doctor");
        Console.WriteLine("  devmemory index");
        Console.WriteLine("  devmemory semantic-search \"your topic\"");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Prints command-specific help for the index command.
    /// </summary>
    private static int PrintIndexHelp()
    {
        Console.WriteLine("DevMemory index");
        Console.WriteLine("---------------");
        Console.WriteLine();

        Console.WriteLine("Usage:");
        Console.WriteLine("  devmemory index");
        Console.WriteLine("  devmemory index --dry-run");
        Console.WriteLine("  devmemory index --force");
        Console.WriteLine("  devmemory index --limit <number>");
        Console.WriteLine("  devmemory index --project <project>");
        Console.WriteLine("  devmemory index --area <area>");
        Console.WriteLine("  devmemory index --tag <tag>");
        Console.WriteLine("  devmemory index --show-text");
        Console.WriteLine();

        Console.WriteLine("Description:");
        Console.WriteLine("  Indexes local memories into the configured vector store.");
        Console.WriteLine("  The local JSON storage remains the source of truth.");
        Console.WriteLine("  The vector store is a derived semantic index and can be rebuilt.");
        Console.WriteLine();

        Console.WriteLine("Options:");
        Console.WriteLine("  --dry-run    Show what would be indexed without generating embeddings or writing vectors.");
        Console.WriteLine("  --force      Rebuild index entries even when memories appear unchanged.");
        Console.WriteLine("  --limit      Limit the number of memories to index.");
        Console.WriteLine("  --project    Index memories for a specific project.");
        Console.WriteLine("  --area       Index memories for a specific area.");
        Console.WriteLine("  --tag        Index memories with a specific tag.");
        Console.WriteLine("  --show-text  Print the generated indexable text during dry-run.");
        Console.WriteLine();

        Console.WriteLine("Examples:");
        Console.WriteLine("  devmemory index --dry-run");
        Console.WriteLine("  devmemory index --dry-run --show-text --limit 1");
        Console.WriteLine("  devmemory index");
        Console.WriteLine("  devmemory index --force");
        Console.WriteLine("  devmemory index --limit 3");
        Console.WriteLine("  devmemory index --project DevMemory");
        Console.WriteLine("  devmemory index --area AI");
        Console.WriteLine("  devmemory index --tag qdrant");
        Console.WriteLine("  devmemory index --project DevMemory --area AI --limit 3");
        Console.WriteLine();

        Console.WriteLine("Requirements:");
        Console.WriteLine("  Real indexing requires a configured embedding provider and vector store.");
        Console.WriteLine("  Dry-run indexing does not require Ollama or Qdrant.");
        Console.WriteLine();

        Console.WriteLine("Typical local configuration:");
        Console.WriteLine("  devmemory config set embedding-provider ollama");
        Console.WriteLine("  devmemory config set vector-store qdrant");
        Console.WriteLine("  devmemory config set ollama-embedding-model nomic-embed-text");
        Console.WriteLine("  devmemory config set qdrant-collection devmemory_memories");
        Console.WriteLine();

        Console.WriteLine("Useful related commands:");
        Console.WriteLine("  devmemory help config");
        Console.WriteLine("  devmemory ai-status");
        Console.WriteLine("  devmemory ai-doctor");
        Console.WriteLine("  devmemory semantic-search \"your topic\"");
        Console.WriteLine("  devmemory ask --rag \"your question\"");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Prints an error for an unknown command-specific help topic.
    /// </summary>
    private static int PrintUnknownHelpTopic(string commandName)
    {
        Console.Error.WriteLine($"Unknown help topic: {commandName}");
        Console.Error.WriteLine();

        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  devmemory help");
        Console.Error.WriteLine("  devmemory help setup");
        Console.Error.WriteLine("  devmemory help config");
        Console.Error.WriteLine("  devmemory help memory");
        Console.Error.WriteLine("  devmemory help ask");
        Console.Error.WriteLine("  devmemory help index");

        return CliExitCodes.InvalidCommand;
    }

}
