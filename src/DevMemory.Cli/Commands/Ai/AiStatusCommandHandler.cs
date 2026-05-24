using DevMemory.Application.Models.Ai.Chat;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;
using DevMemory.Cli.CommandLine;
using DevMemory.Infrastructure;

namespace DevMemory.Cli.Commands.Ai;

public sealed class AiStatusCommandHandler : ICommandHandler
{
    private readonly Func<AiRuntimeOptions> _optionsFactory;

    public AiStatusCommandHandler()
        : this(AiRuntimeOptionsProvider.GetOptions)
    {
    }

    public AiStatusCommandHandler(Func<AiRuntimeOptions> optionsFactory)
    {
        _optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
    }

    public string Name => "ai-status";

    public int Execute(string[] args)
    {
        var options = _optionsFactory();

        Console.WriteLine("AI runtime status");
        Console.WriteLine("-----------------");
        Console.WriteLine();

        PrintChatStatus(options.Chat);
        Console.WriteLine();

        PrintEmbeddingStatus(options.Embedding);
        Console.WriteLine();

        PrintVectorStoreStatus(options.VectorStore);
        Console.WriteLine();

        Console.WriteLine($"Chat enabled: {FormatBoolean(options.IsChatEnabled)}");
        Console.WriteLine($"Semantic search enabled: {FormatBoolean(options.IsSemanticSearchEnabled)}");
        Console.WriteLine($"Full RAG enabled: {FormatBoolean(options.IsFullRagEnabled)}");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Prints the configured chat provider status.
    /// </summary>
    private static void PrintChatStatus(ChatProviderOptions options)
    {
        Console.WriteLine($"Chat provider: {options.Provider}");

        if (options.IsOllama)
        {
            Console.WriteLine($"Chat endpoint: {FormatOptional(options.OllamaEndpoint)}");
            Console.WriteLine($"Chat model: {FormatOptional(options.OllamaChatModel)}");

            return;
        }

        if (options.IsOpenAi)
        {
            Console.WriteLine($"Chat model: {FormatOptional(options.OpenAiChatModel)}");
            Console.WriteLine(
                $"OpenAI API key configured: {FormatBoolean(!string.IsNullOrWhiteSpace(options.OpenAiApiKey))}");

            return;
        }

        if (options.IsGemini)
        {
            Console.WriteLine($"Chat model: {FormatOptional(options.GeminiChatModel)}");
            Console.WriteLine(
                $"Gemini API key configured: {FormatBoolean(!string.IsNullOrWhiteSpace(options.GeminiApiKey))}");

            return;
        }

        if (options.IsAnthropic)
        {
            Console.WriteLine($"Chat model: {FormatOptional(options.AnthropicChatModel)}");
            Console.WriteLine(
                $"Anthropic API key configured: {FormatBoolean(!string.IsNullOrWhiteSpace(options.AnthropicApiKey))}");
        }
    }

    /// <summary>
    /// Prints the configured embedding provider status.
    /// </summary>
    private static void PrintEmbeddingStatus(EmbeddingProviderOptions options)
    {
        Console.WriteLine($"Embedding provider: {options.Provider}");

        if (options.IsOllama)
        {
            Console.WriteLine($"Embedding endpoint: {FormatOptional(options.OllamaEndpoint)}");
            Console.WriteLine($"Embedding model: {FormatOptional(options.OllamaEmbeddingModel)}");

            return;
        }

        if (options.IsOpenAi)
        {
            Console.WriteLine($"Embedding model: {FormatOptional(options.OpenAiEmbeddingModel)}");
            Console.WriteLine(
                $"OpenAI API key configured: {FormatBoolean(!string.IsNullOrWhiteSpace(options.OpenAiApiKey))}");

            return;
        }

        if (options.IsGemini)
        {
            Console.WriteLine($"Embedding model: {FormatOptional(options.GeminiEmbeddingModel)}");
            Console.WriteLine(
                $"Gemini API key configured: {FormatBoolean(!string.IsNullOrWhiteSpace(options.GeminiApiKey))}");
        }
    }

    /// <summary>
    /// Prints the configured vector store status.
    /// </summary>
    private static void PrintVectorStoreStatus(VectorStoreOptions options)
    {
        Console.WriteLine($"Vector store: {options.Provider}");

        if (options.IsQdrant)
        {
            Console.WriteLine($"Qdrant endpoint: {FormatOptional(options.QdrantEndpoint)}");
            Console.WriteLine($"Qdrant collection: {FormatOptional(options.QdrantCollection)}");
        }
    }

    /// <summary>
    /// Formats a boolean value for CLI output.
    /// </summary>
    private static string FormatBoolean(bool value)
    {
        return value ? "yes" : "no";
    }

    /// <summary>
    /// Formats an optional string value for CLI output.
    /// </summary>
    private static string FormatOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }
}
