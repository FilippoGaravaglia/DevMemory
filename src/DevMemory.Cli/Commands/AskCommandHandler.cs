using DevMemory.Application.Models.Ai;
using DevMemory.Cli.CommandLine;
using DevMemory.Infrastructure;

namespace DevMemory.Cli.Commands;

public sealed class AskCommandHandler : ICommandHandler
{
    private readonly Func<AiRuntimeOptions> _optionsFactory;

    public AskCommandHandler()
        : this(AiRuntimeOptionsProvider.GetOptions)
    {
    }

    public AskCommandHandler(Func<AiRuntimeOptions> optionsFactory)
    {
        _optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
    }

    public string Name => "ask";

    public int Execute(string[] args)
    {
        var question = BuildQuestion(args);
        var options = _optionsFactory();

        if (!options.IsChatEnabled)
        {
            Console.Error.WriteLine("AI chat is not configured.");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Configure a chat provider before using this command.");
            Console.Error.WriteLine("Example:");
            Console.Error.WriteLine("  DEVMEMORY_CHAT_PROVIDER=ollama devmemory ai-status");

            return CliExitCodes.Failure;
        }

        Console.WriteLine("DevMemory AI ask");
        Console.WriteLine("----------------");
        Console.WriteLine();
        Console.WriteLine($"Question: {question}");
        Console.WriteLine();
        Console.WriteLine($"Chat provider: {options.Chat.Provider}");
        Console.WriteLine($"Chat model: {ResolveChatModel(options.Chat)}");
        Console.WriteLine();

        Console.Error.WriteLine(
            "AI chat execution is not implemented yet. The command is ready, but no provider adapter is wired.");

        return CliExitCodes.Failure;
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

        return "-";
    }

    /// <summary>
    /// Formats an optional string value for CLI output.
    /// </summary>
    private static string FormatOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }
}
