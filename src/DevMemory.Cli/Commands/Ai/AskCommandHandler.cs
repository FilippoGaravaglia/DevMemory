using System.Globalization;
using DevMemory.Application.Abstractions;
using DevMemory.Application.Ai.Rag;
using DevMemory.Application.Ai.Search;
using DevMemory.Application.Models.Ai.Chat;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;
using DevMemory.Application.Models.Git;
using DevMemory.Cli.CommandLine;
using DevMemory.Infrastructure;
using DevMemory.Infrastructure.Ai.Factories;

namespace DevMemory.Cli.Commands.Ai;

public sealed class AskCommandHandler : ICommandHandler
{
    private const int DefaultRagLimit = 5;

    private readonly Func<AiRuntimeOptions> _optionsFactory;
    private readonly Func<AiRuntimeOptions, IChatCompletionService?> _chatServiceFactory;
    private readonly Func<AiRuntimeOptions, IEmbeddingService?> _embeddingServiceFactory;
    private readonly Func<AiRuntimeOptions, IVectorMemoryStore?> _vectorMemoryStoreFactory;
    private readonly Func<IEmbeddingService, IVectorMemoryStore, IChatCompletionService, MemoryRagAnswerService> _ragAnswerServiceFactory;

    public AskCommandHandler()
        : this(
            AiRuntimeOptionsProvider.GetOptions,
            ChatCompletionServiceFactory.Create,
            EmbeddingServiceFactory.Create,
            VectorMemoryStoreFactory.Create,
            static (embeddingService, vectorMemoryStore, chatService) =>
                new MemoryRagAnswerService(
                    new MemorySemanticSearchService(embeddingService, vectorMemoryStore),
                    chatService))
    {
    }

    public AskCommandHandler(Func<AiRuntimeOptions> optionsFactory)
        : this(optionsFactory, _ => null)
    {
    }

    public AskCommandHandler(
        Func<AiRuntimeOptions> optionsFactory,
        Func<AiRuntimeOptions, IChatCompletionService?> chatServiceFactory)
        : this(
            optionsFactory,
            chatServiceFactory,
            _ => null,
            _ => null,
            static (embeddingService, vectorMemoryStore, chatService) =>
                new MemoryRagAnswerService(
                    new MemorySemanticSearchService(embeddingService, vectorMemoryStore),
                    chatService))
    {
    }

    public AskCommandHandler(
        Func<AiRuntimeOptions> optionsFactory,
        Func<AiRuntimeOptions, IChatCompletionService?> chatServiceFactory,
        Func<AiRuntimeOptions, IEmbeddingService?> embeddingServiceFactory,
        Func<AiRuntimeOptions, IVectorMemoryStore?> vectorMemoryStoreFactory,
        Func<IEmbeddingService, IVectorMemoryStore, IChatCompletionService, MemoryRagAnswerService> ragAnswerServiceFactory)
    {
        _optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
        _chatServiceFactory = chatServiceFactory ?? throw new ArgumentNullException(nameof(chatServiceFactory));
        _embeddingServiceFactory = embeddingServiceFactory
            ?? throw new ArgumentNullException(nameof(embeddingServiceFactory));
        _vectorMemoryStoreFactory = vectorMemoryStoreFactory
            ?? throw new ArgumentNullException(nameof(vectorMemoryStoreFactory));
        _ragAnswerServiceFactory = ragAnswerServiceFactory
            ?? throw new ArgumentNullException(nameof(ragAnswerServiceFactory));
    }

    public string Name => "ask";

    public int Execute(string[] args)
    {
        var request = ParseRequest(args);
        var options = _optionsFactory();

        if (!options.IsChatEnabled)
        {
            PrintChatNotConfigured();

            return CliExitCodes.Failure;
        }

        var chatService = _chatServiceFactory(options);

        if (chatService is null)
        {
            PrintChatAdapterNotAvailable(options.Chat.Provider);

            return CliExitCodes.Failure;
        }

        try
        {
            return request.UseRag
                ? ExecuteRagAsk(request, options, (IChatCompletionService)chatService)
                : ExecutePlainAsk(request, options, (IChatCompletionService)chatService);
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            Console.Error.WriteLine(request.UseRag ? "AI RAG request failed." : "AI chat request failed.");
            Console.Error.WriteLine(ex.Message);

            return CliExitCodes.Failure;
        }
        finally
        {
            DisposeIfRequired(chatService);
        }
    }

    /// <summary>
    /// Executes a plain chat request without memory retrieval.
    /// </summary>
    private static int ExecutePlainAsk(
        AskCliRequest request,
        AiRuntimeOptions options,
        IChatCompletionService chatService)
    {
        var chatRequest = BuildChatCompletionRequest(request.Question, options.Chat);

        var response = chatService
            .CompleteAsync(chatRequest, CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        PrintAnswer(request.Question, response);

        return CliExitCodes.Success;
    }

    /// <summary>
    /// Executes a retrieval-augmented chat request using indexed memory context.
    /// </summary>
    private int ExecuteRagAsk(
        AskCliRequest request,
        AiRuntimeOptions options,
        IChatCompletionService chatService)
    {
        if (!options.IsFullRagEnabled)
        {
            PrintRagNotConfigured();

            return CliExitCodes.Failure;
        }

        var embeddingService = _embeddingServiceFactory(options);

        if (embeddingService is null)
        {
            PrintEmbeddingAdapterNotAvailable(options.Embedding.Provider);

            return CliExitCodes.Failure;
        }

        var vectorMemoryStore = _vectorMemoryStoreFactory(options);

        if (vectorMemoryStore is null)
        {
            DisposeIfRequired(embeddingService);
            PrintVectorStoreAdapterNotAvailable(options.VectorStore.Provider);

            return CliExitCodes.Failure;
        }

        try
        {
            var ragAnswerService = _ragAnswerServiceFactory(
                embeddingService,
                vectorMemoryStore,
                chatService);

            var result = ragAnswerService
                .AnswerAsync(
                    request.Question,
                    ResolveEmbeddingModel(options.Embedding),
                    ResolveChatModel(options.Chat),
                    request.RagLimit,
                    CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            PrintRagAnswer(result, request.ShowContext);

            return CliExitCodes.Success;
        }
        finally
        {
            DisposeIfRequired(embeddingService);
            DisposeIfRequired(vectorMemoryStore);
        }
    }

    /// <summary>
    /// Parses the ask command request.
    /// </summary>
    private static AskCliRequest ParseRequest(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Usage: devmemory ask [--rag] [--show-context] [--limit <number>] <question>");
        }

        var useRag = false;
        var showContext = false;
        var ragLimit = DefaultRagLimit;
        var questionParts = new List<string>();

        for (var index = 1; index < args.Length; index++)
        {
            var value = args[index];

            if (value.Equals("--rag", StringComparison.OrdinalIgnoreCase))
            {
                useRag = true;
                continue;
            }

            if (value.Equals("--show-context", StringComparison.OrdinalIgnoreCase))
            {
                useRag = true;
                showContext = true;
                continue;
            }

            if (value.Equals("--limit", StringComparison.OrdinalIgnoreCase))
            {
                var limitValue = CommandOptions.ReadOptionValue(args, ref index, "--limit");

                if (!int.TryParse(limitValue, out ragLimit) || ragLimit <= 0)
                {
                    throw new ArgumentException("Option --limit must be a positive integer.");
                }

                continue;
            }

            questionParts.Add(value);
        }

        var question = string.Join(' ', questionParts).Trim();

        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("Usage: devmemory ask [--rag] [--show-context] [--limit <number>] <question>");
        }

        return new AskCliRequest(question, useRag, showContext, ragLimit);
    }

    /// <summary>
    /// Builds the provider-independent chat completion request.
    /// </summary>
    private static ChatCompletionRequest BuildChatCompletionRequest(
        string question,
        ChatProviderOptions options)
    {
        return new ChatCompletionRequest
        {
            Model = ResolveChatModel(options),
            Temperature = 0.2m,
            Messages =
            [
                new ChatCompletionMessage
                {
                    Role = ChatMessageRoles.System,
                    Content = "You are DevMemory, a local-first developer memory assistant. Answer clearly and concisely."
                },
                new ChatCompletionMessage
                {
                    Role = ChatMessageRoles.User,
                    Content = question
                }
            ]
        };
    }

    /// <summary>
    /// Resolves the configured chat model for the selected provider.
    /// </summary>
    private static string ResolveChatModel(ChatProviderOptions options)
    {
        if (options.IsOllama)
        {
            return FormatOptional(options.OllamaChatModel);
        }

        if (options.IsOpenAi)
        {
            return FormatOptional(options.OpenAiChatModel);
        }

        if (options.IsGemini)
        {
            return FormatOptional(options.GeminiChatModel);
        }

        if (options.IsAnthropic)
        {
            return FormatOptional(options.AnthropicChatModel);
        }

        return string.Empty;
    }

    /// <summary>
    /// Resolves the configured embedding model for the selected provider.
    /// </summary>
    private static string ResolveEmbeddingModel(EmbeddingProviderOptions options)
    {
        if (options.IsOllama)
        {
            return FormatOptional(options.OllamaEmbeddingModel);
        }

        if (options.IsOpenAi)
        {
            return FormatOptional(options.OpenAiEmbeddingModel);
        }

        if (options.IsGemini)
        {
            return FormatOptional(options.GeminiEmbeddingModel);
        }

        return string.Empty;
    }

    /// <summary>
    /// Prints a user-friendly message when no chat provider is configured.
    /// </summary>
    private static void PrintChatNotConfigured()
    {
        Console.Error.WriteLine("AI chat is not configured.");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Configure a chat provider before using this command.");
        Console.Error.WriteLine("Example:");
        Console.Error.WriteLine("  DEVMEMORY_CHAT_PROVIDER=ollama devmemory ai-status");
    }

    /// <summary>
    /// Prints a user-friendly message when the selected provider does not have an adapter yet.
    /// </summary>
    private static void PrintChatAdapterNotAvailable(string provider)
    {
        Console.Error.WriteLine($"AI chat provider '{provider}' is configured, but no adapter is available yet.");
        Console.Error.WriteLine("Currently implemented chat providers: ollama");
    }

    /// <summary>
    /// Prints a user-friendly message when RAG is not fully configured.
    /// </summary>
    private static void PrintRagNotConfigured()
    {
        Console.Error.WriteLine("RAG is not configured.");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Configure chat, embedding, and vector store providers before using ask --rag.");
        Console.Error.WriteLine("Example:");
        Console.Error.WriteLine("  DEVMEMORY_CHAT_PROVIDER=ollama DEVMEMORY_EMBEDDING_PROVIDER=ollama DEVMEMORY_VECTOR_STORE=qdrant devmemory ai-status");
    }

    /// <summary>
    /// Prints a user-friendly message when the selected embedding provider has no adapter.
    /// </summary>
    private static void PrintEmbeddingAdapterNotAvailable(string provider)
    {
        Console.Error.WriteLine($"Embedding provider '{provider}' is configured, but no adapter is available yet.");
        Console.Error.WriteLine("Currently implemented embedding providers: ollama");
    }

    /// <summary>
    /// Prints a user-friendly message when the selected vector store has no adapter.
    /// </summary>
    private static void PrintVectorStoreAdapterNotAvailable(string provider)
    {
        Console.Error.WriteLine($"Vector store '{provider}' is configured, but no adapter is available yet.");
        Console.Error.WriteLine("Currently implemented vector stores: qdrant");
    }

    /// <summary>
    /// Prints the AI answer returned by the configured provider.
    /// </summary>
    private static void PrintAnswer(string question, ChatCompletionResponse response)
    {
        Console.WriteLine("DevMemory AI answer");
        Console.WriteLine("-------------------");
        Console.WriteLine();
        Console.WriteLine($"Provider: {response.Provider}");
        Console.WriteLine($"Model: {response.Model}");
        Console.WriteLine($"Question: {question}");
        Console.WriteLine();
        Console.WriteLine("Answer:");
        Console.WriteLine(response.Content);
    }

    /// <summary>
    /// Prints the retrieval-augmented answer returned by the configured provider.
    /// </summary>
    private static void PrintRagAnswer(MemoryRagAnswerResult result, bool showContext)
    {
        Console.WriteLine("DevMemory RAG answer");
        Console.WriteLine("--------------------");
        Console.WriteLine();
        Console.WriteLine($"Provider: {result.Provider}");
        Console.WriteLine($"Model: {result.Model}");
        Console.WriteLine($"Question: {result.Question}");
        Console.WriteLine($"Context items: {result.ContextItemsCount}");
        Console.WriteLine();
        Console.WriteLine("Answer:");
        Console.WriteLine(result.Answer);

        if (showContext)
        {
            PrintRagContext((IReadOnlyCollection<VectorMemorySearchResult>)result.ContextResults);
        }
    }

    /// <summary>
    /// Prints the memory context used to produce a RAG answer.
    /// </summary>
    private static void PrintRagContext(IReadOnlyCollection<VectorMemorySearchResult> contextResults)
    {
        Console.WriteLine();
        Console.WriteLine("Context:");
        Console.WriteLine();

        if (contextResults.Count == 0)
        {
            Console.WriteLine("No memory context was used.");

            return;
        }

        var position = 1;

        foreach (var contextResult in contextResults.OrderByDescending(result => result.Score))
        {
            Console.WriteLine($"{position}. {FormatOptional(contextResult.Title)}");
            Console.WriteLine($"   MemoryId: {contextResult.MemoryId:D}");
            Console.WriteLine($"   Project: {FormatOptional(contextResult.Project)}");
            Console.WriteLine($"   Area: {FormatOptional(contextResult.Area)}");
            Console.WriteLine($"   Score: {contextResult.Score.ToString(CultureInfo.InvariantCulture)}");

            var preview = CreateTextPreview(contextResult.Text);

            if (!string.IsNullOrWhiteSpace(preview))
            {
                Console.WriteLine($"   Preview: {preview}");
            }

            Console.WriteLine();

            position++;
        }
    }

    /// <summary>
    /// Creates a short single-line preview from a retrieved memory text.
    /// </summary>
    private static string CreateTextPreview(string text)
    {
        const int maxLength = 240;

        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text
            .ReplaceLineEndings(" ")
            .Trim();

        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength] + "...";
    }

    /// <summary>
    /// Formats an optional string value for CLI output.
    /// </summary>
    private static string FormatOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value;
    }

    /// <summary>
    /// Disposes an object when it implements IDisposable.
    /// </summary>
    private static void DisposeIfRequired(object? instance)
    {
        if (instance is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private sealed record AskCliRequest(
        string Question,
        bool UseRag,
        bool ShowContext,
        int RagLimit);
}
