using DevMemory.Cli.CommandLine;
using DevMemory.Infrastructure;

namespace DevMemory.Cli.Commands.System;

/// <summary>
/// Manages persistent DevMemory local configuration.
/// </summary>
public sealed class ConfigCommandHandler : ICommandHandler
{
    private static readonly Dictionary<string, Func<AiRuntimeConfiguration, string?>> Readers =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["chat-provider"] = configuration => configuration.ChatProvider,
            ["embedding-provider"] = configuration => configuration.EmbeddingProvider,
            ["vector-store"] = configuration => configuration.VectorStore,
            ["ollama-endpoint"] = configuration => configuration.OllamaEndpoint,
            ["ollama-chat-model"] = configuration => configuration.OllamaChatModel,
            ["ollama-embedding-model"] = configuration => configuration.OllamaEmbeddingModel,
            ["qdrant-endpoint"] = configuration => configuration.QdrantEndpoint,
            ["qdrant-collection"] = configuration => configuration.QdrantCollection,
            ["openai-chat-model"] = configuration => configuration.OpenAiChatModel,
            ["openai-embedding-model"] = configuration => configuration.OpenAiEmbeddingModel,
            ["gemini-chat-model"] = configuration => configuration.GeminiChatModel,
            ["gemini-embedding-model"] = configuration => configuration.GeminiEmbeddingModel,
            ["anthropic-chat-model"] = configuration => configuration.AnthropicChatModel
        };

    private readonly AiRuntimeConfigurationStore _configurationStore;

    public ConfigCommandHandler()
        : this(new AiRuntimeConfigurationStore())
    {
    }

    public ConfigCommandHandler(AiRuntimeConfigurationStore configurationStore)
    {
        _configurationStore = configurationStore
            ?? throw new ArgumentNullException(nameof(configurationStore));
    }

    public string Name => "config";

    public int Execute(string[] args)
    {
        if (args.Length < 2)
        {
            PrintUsage();

            return CliExitCodes.Failure;
        }

        var subCommand = args[1];

        if (subCommand.Equals("show", StringComparison.OrdinalIgnoreCase))
        {
            return ExecuteShow();
        }

        if (subCommand.Equals("set", StringComparison.OrdinalIgnoreCase))
        {
            return ExecuteSet(args);
        }

        if (subCommand.Equals("reset", StringComparison.OrdinalIgnoreCase))
        {
            return ExecuteReset();
        }

        PrintUsage();

        return CliExitCodes.Failure;
    }

    /// <summary>
    /// Prints the current persisted DevMemory configuration.
    /// </summary>
    private int ExecuteShow()
    {
        var configuration = _configurationStore.Load();

        Console.WriteLine("DevMemory configuration");
        Console.WriteLine("-----------------------");
        Console.WriteLine();
        Console.WriteLine($"File: {_configurationStore.ConfigFilePath}");
        Console.WriteLine();

        foreach (var key in Readers.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"{key}: {FormatValue(Readers[key](configuration))}");
        }

        Console.WriteLine();
        Console.WriteLine("Effective AI runtime:");
        Console.WriteLine("---------------------");

        var options = AiRuntimeOptionsProvider.GetOptions(configuration);

        Console.WriteLine($"Chat provider: {options.Chat.Provider}");
        Console.WriteLine($"Embedding provider: {options.Embedding.Provider}");
        Console.WriteLine($"Vector store: {options.VectorStore.Provider}");
        Console.WriteLine($"Ollama endpoint: {options.Chat.OllamaEndpoint}");
        Console.WriteLine($"Ollama chat model: {options.Chat.OllamaChatModel}");
        Console.WriteLine($"Ollama embedding model: {options.Embedding.OllamaEmbeddingModel}");
        Console.WriteLine($"Qdrant endpoint: {options.VectorStore.QdrantEndpoint}");
        Console.WriteLine($"Qdrant collection: {options.VectorStore.QdrantCollection}");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Sets a persistent DevMemory configuration value.
    /// </summary>
    private int ExecuteSet(string[] args)
    {
        if (args.Length != 4)
        {
            PrintUsage();

            return CliExitCodes.Failure;
        }

        var key = args[2].Trim();
        var value = args[3].Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            Console.Error.WriteLine("Configuration value cannot be empty.");

            return CliExitCodes.Failure;
        }

        if (!Readers.ContainsKey(key))
        {
            Console.Error.WriteLine($"Unknown configuration key: {key}");
            Console.Error.WriteLine();

            PrintSupportedKeys(Console.Error);

            return CliExitCodes.Failure;
        }

        var current = _configurationStore.Load();
        var updated = SetValue(current, key, value);

        _configurationStore.Save(updated);

        Console.WriteLine("DevMemory configuration updated.");
        Console.WriteLine($"File: {_configurationStore.ConfigFilePath}");
        Console.WriteLine($"{key}: {value}");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Resets the persistent DevMemory configuration file.
    /// </summary>
    private int ExecuteReset()
    {
        _configurationStore.Reset();

        Console.WriteLine("DevMemory configuration reset.");
        Console.WriteLine($"File: {_configurationStore.ConfigFilePath}");

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Returns an updated configuration with the selected key changed.
    /// </summary>
    private static AiRuntimeConfiguration SetValue(
        AiRuntimeConfiguration configuration,
        string key,
        string value)
    {
        return key.ToLowerInvariant() switch
        {
            "chat-provider" => Copy(configuration, chatProvider: value),
            "embedding-provider" => Copy(configuration, embeddingProvider: value),
            "vector-store" => Copy(configuration, vectorStore: value),
            "ollama-endpoint" => Copy(configuration, ollamaEndpoint: value),
            "ollama-chat-model" => Copy(configuration, ollamaChatModel: value),
            "ollama-embedding-model" => Copy(configuration, ollamaEmbeddingModel: value),
            "qdrant-endpoint" => Copy(configuration, qdrantEndpoint: value),
            "qdrant-collection" => Copy(configuration, qdrantCollection: value),
            "openai-chat-model" => Copy(configuration, openAiChatModel: value),
            "openai-embedding-model" => Copy(configuration, openAiEmbeddingModel: value),
            "gemini-chat-model" => Copy(configuration, geminiChatModel: value),
            "gemini-embedding-model" => Copy(configuration, geminiEmbeddingModel: value),
            "anthropic-chat-model" => Copy(configuration, anthropicChatModel: value),
            _ => configuration
        };
    }

    /// <summary>
    /// Creates a copy of the configuration with optional overrides.
    /// </summary>
    private static AiRuntimeConfiguration Copy(
        AiRuntimeConfiguration configuration,
        string? chatProvider = null,
        string? embeddingProvider = null,
        string? vectorStore = null,
        string? ollamaEndpoint = null,
        string? ollamaChatModel = null,
        string? ollamaEmbeddingModel = null,
        string? qdrantEndpoint = null,
        string? qdrantCollection = null,
        string? openAiChatModel = null,
        string? openAiEmbeddingModel = null,
        string? geminiChatModel = null,
        string? geminiEmbeddingModel = null,
        string? anthropicChatModel = null)
    {
        return new AiRuntimeConfiguration
        {
            ChatProvider = chatProvider ?? configuration.ChatProvider,
            EmbeddingProvider = embeddingProvider ?? configuration.EmbeddingProvider,
            VectorStore = vectorStore ?? configuration.VectorStore,
            OllamaEndpoint = ollamaEndpoint ?? configuration.OllamaEndpoint,
            OllamaChatModel = ollamaChatModel ?? configuration.OllamaChatModel,
            OllamaEmbeddingModel = ollamaEmbeddingModel ?? configuration.OllamaEmbeddingModel,
            QdrantEndpoint = qdrantEndpoint ?? configuration.QdrantEndpoint,
            QdrantCollection = qdrantCollection ?? configuration.QdrantCollection,
            OpenAiChatModel = openAiChatModel ?? configuration.OpenAiChatModel,
            OpenAiEmbeddingModel = openAiEmbeddingModel ?? configuration.OpenAiEmbeddingModel,
            GeminiChatModel = geminiChatModel ?? configuration.GeminiChatModel,
            GeminiEmbeddingModel = geminiEmbeddingModel ?? configuration.GeminiEmbeddingModel,
            AnthropicChatModel = anthropicChatModel ?? configuration.AnthropicChatModel
        };
    }

    /// <summary>
    /// Prints command usage.
    /// </summary>
    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  devmemory config show");
        Console.WriteLine("  devmemory config set <key> <value>");
        Console.WriteLine("  devmemory config reset");
        Console.WriteLine();

        PrintSupportedKeys(Console.Out);
    }

    /// <summary>
    /// Prints supported configuration keys.
    /// </summary>
    private static void PrintSupportedKeys(TextWriter writer)
    {
        writer.WriteLine("Supported keys:");

        foreach (var key in Readers.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase))
        {
            writer.WriteLine($"  {key}");
        }
    }

    /// <summary>
    /// Formats a persisted configuration value.
    /// </summary>
    private static string FormatValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }
}
