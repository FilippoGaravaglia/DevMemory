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
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- ai-status");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- ask <question>");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- index");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- semantic-search <query> [--limit <number>]");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- version");
        Console.WriteLine("  dotnet run --project src/DevMemory.Cli -- --version");
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
        Console.WriteLine("  devmemory ai-status");
        Console.WriteLine("  devmemory ask <question>");
        Console.WriteLine("  devmemory index");
        Console.WriteLine("  devmemory semantic-search <query> [--limit <number>]");
        Console.WriteLine("  devmemory version");
        Console.WriteLine("  devmemory --version");
        Console.WriteLine("  devmemory -v");
        Console.WriteLine();

        Console.WriteLine("Commands:");
        Console.WriteLine("  add              Create a new structured memory.");
        Console.WriteLine("  list             List saved memories.");
        Console.WriteLine("  search           Search memories by text and optional filters.");
        Console.WriteLine("  show             Show a memory by id.");
        Console.WriteLine("  storage          Show the current storage file path.");
        Console.WriteLine("  markdown         Show the Markdown export directory.");
        Console.WriteLine("  git-status       Inspect the current or selected Git repository.");
        Console.WriteLine("  learn-from-git   Create a memory draft from Git context.");
        Console.WriteLine("  graph-export     Export the memory graph as JSON.");
        Console.WriteLine("  graph-view       Generate the local HTML graph view.");
        Console.WriteLine("  ai-status        Show the current AI/RAG runtime configuration status.");
        Console.WriteLine("  ask              Ask a question using the configured AI chat provider.");
        Console.WriteLine("  index            Index local memories into the configured vector store.");
        Console.WriteLine("  semantic-search  Search indexed memories using semantic similarity.");
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
        Console.WriteLine("  devmemory git-status");
        Console.WriteLine("  devmemory learn-from-git");
        Console.WriteLine("  devmemory graph-export");
        Console.WriteLine("  devmemory graph-view");
        Console.WriteLine("  devmemory ai-status");
        Console.WriteLine("  devmemory ask \"What did I change last time in this area?\"");
        Console.WriteLine("  devmemory index");
        Console.WriteLine("  devmemory semantic-search \"estimate revision cloning\"");
        Console.WriteLine("  devmemory semantic-search \"mongodb mapping issue\" --limit 3");
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

        Console.WriteLine("Environment examples:");
        Console.WriteLine("  DEVMEMORY_HOME=~/devmemory-work devmemory storage");
        Console.WriteLine("  DEVMEMORY_CHAT_PROVIDER=ollama devmemory ai-status");
        Console.WriteLine("  DEVMEMORY_CHAT_PROVIDER=ollama devmemory ask \"What did I change last time?\"");
        Console.WriteLine("  DEVMEMORY_EMBEDDING_PROVIDER=ollama DEVMEMORY_VECTOR_STORE=qdrant devmemory index");
        Console.WriteLine("  DEVMEMORY_EMBEDDING_PROVIDER=ollama DEVMEMORY_VECTOR_STORE=qdrant devmemory semantic-search \"estimate revision\"");

        return CliExitCodes.Success;
    }
}
