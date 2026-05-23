using DevMemory.Application.Abstractions.Ai;
using DevMemory.Application.Models.Ai;
using DevMemory.Cli.CommandLine;
using DevMemory.Infrastructure;
using DevMemory.Infrastructure.Ai;

namespace DevMemory.Cli.Commands;

public sealed class AskCommandHandler : ICommandHandler
{
    private readonly Func<AiRuntimeOptions> _optionsFactory;
    private readonly Func<AiRuntimeOptions, IChatCompletionService?> _chatServiceFactory;

    public AskCommandHandler()
        : this(AiRuntimeOptionsProvider.GetOptions, ChatCompletionServiceFactory.Create)
    {
    }

    public AskCommandHandler(Func<AiRuntimeOptions> optionsFactory)
        : this(optionsFactory, _ => null)
    {
    }

    public AskCommandHandler(
        Func<AiRuntimeOptions> optionsFactory,
        Func<AiRuntimeOptions, IChatCompletionService?> chatServiceFactory)
    {
        _optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
        _chatServiceFactory = chatServiceFactory ?? throw new ArgumentNullException(nameof(chatServiceFactory));
    }

    public string Name => "ask";

    public int Execute(string[] args)
    {
        var question = BuildQuestion(args);
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
            var request = BuildChatCompletionRequest(question, options.Chat);
            var response = chatService
                .CompleteAsync(request, CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            PrintAnswer(question, response);

            return CliExitCodes.Success;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            Console.Error.WriteLine("AI chat request failed.");
            Console.Error.WriteLine(ex.Message);

            return CliExitCodes.Failure;
        }
        finally
        {
            if (chatService is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    /// <summary>
    /// Builds the question text from command-line arguments.
    /// </summary>
    private static string BuildQuestion(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("Usage: devmemory ask <question>");
        }

        var questionParts = new List<string>();

        for (var index = 1; index < args.Length; index++)
        {
            questionParts.Add(args[index]);
        }

        var question = string.Join(' ', questionParts).Trim();

        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("Usage: devmemory ask <question>");
        }

        return question;
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
    /// Formats an optional string value for CLI output.
    /// </summary>
    private static string FormatOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value;
    }
}
