using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Commands.System;

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
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- edit <memory-id> [options]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- delete <memory-id> [--yes]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- storage");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- markdown");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- git-status [--path <repository-path>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- learn-from-git [--path <repository-path>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- graph-export [--output <file-path>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- graph-view [--output <file-path>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- ai-status");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- ai-doctor");
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
        Console.WriteLine();

        Console.WriteLine("Installed tool usage:");
        Console.WriteLine("  devmemory add");
        Console.WriteLine("  devmemory list");
        Console.WriteLine("  devmemory search <query> [--project <project>] [--area <area>] [--tag <tag>]");
        Console.WriteLine("  devmemory show <memory-id>");
        Console.WriteLine("  devmemory edit <memory-id> [options]");
        Console.WriteLine("  devmemory delete <memory-id> [--yes]");
        Console.WriteLine("  devmemory storage");
        Console.WriteLine("  devmemory markdown");
        Console.WriteLine("  devmemory git-status [--path <repository-path>]");
        Console.WriteLine("  devmemory learn-from-git [--path <repository-path>]");
        Console.WriteLine("  devmemory graph-export [--output <file-path>]");
        Console.WriteLine("  devmemory graph-view [--output <file-path>]");
        Console.WriteLine("  devmemory ai-status");
        Console.WriteLine("  devmemory ai-doctor");
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
        Console.WriteLine();

        Console.WriteLine("Commands:");
        Console.WriteLine("  add              Create a new structured memory.");
        Console.WriteLine("  list             List saved memories.");
        Console.WriteLine("  search           Search memories by text and optional filters.");
        Console.WriteLine("  show             Show a memory by id.");
        Console.WriteLine("  edit             Edit an existing memory by id.");
        Console.WriteLine("  delete           Delete a memory by id from local storage.");
        Console.WriteLine("  storage          Show the current storage file path.");
        Console.WriteLine("  markdown         Show the Markdown export directory.");
        Console.WriteLine("  git-status       Inspect the current or selected Git repository.");
        Console.WriteLine("  learn-from-git   Create a memory draft from Git context.");
        Console.WriteLine("  graph-export     Export the memory graph as JSON.");
        Console.WriteLine("  graph-view       Generate the local HTML graph view.");
        Console.WriteLine("  ai-status        Show the current AI/RAG runtime configuration status.");
        Console.WriteLine("  ai-doctor        Diagnose the local AI runtime configuration.");
        Console.WriteLine("  ask              Ask a question using the configured AI chat provider.");
        Console.WriteLine("  index            Index local memories into the configured vector store.");
        Console.WriteLine("  semantic-search  Search indexed memories using semantic similarity.");
        Console.WriteLine("  related         Find indexed memories semantically related to a memory.");
        Console.WriteLine("  config           Show, set or reset persistent DevMemory configuration.");
        Console.WriteLine("  version          Show the current DevMemory version.");
        Console.WriteLine("  help             Show this help message.");
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
        Console.WriteLine("  devmemory git-status");
        Console.WriteLine("  devmemory learn-from-git");
        Console.WriteLine("  devmemory graph-export");
        Console.WriteLine("  devmemory graph-view");
        Console.WriteLine("  devmemory ai-status");
        Console.WriteLine("  devmemory ai-doctor");
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

        return CliExitCodes.Success;
    }
}
