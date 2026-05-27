using DevMemory.Application.Abstractions;
using DevMemory.Application.Ai.Search;
using DevMemory.Application.Models.Ai.Chat;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Git;

namespace DevMemory.Application.Ai.Rag;

/// <summary>
/// Orchestrates retrieval-augmented answers over indexed developer memories.
/// </summary>
public sealed class MemoryRagAnswerService
{
    private readonly MemorySemanticSearchService _semanticSearchService;
    private readonly IChatCompletionService _chatCompletionService;

    public MemoryRagAnswerService(
        MemorySemanticSearchService semanticSearchService,
        IChatCompletionService chatCompletionService)
    {
        _semanticSearchService = semanticSearchService
            ?? throw new ArgumentNullException(nameof(semanticSearchService));
        _chatCompletionService = chatCompletionService
            ?? throw new ArgumentNullException(nameof(chatCompletionService));
    }

    public async Task<MemoryRagAnswerResult> AnswerAsync(
        string question,
        string embeddingModel,
        string chatModel,
        int contextLimit,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("RAG question cannot be empty.", nameof(question));
        }

        if (string.IsNullOrWhiteSpace(embeddingModel))
        {
            throw new ArgumentException("Embedding model cannot be empty.", nameof(embeddingModel));
        }

        if (string.IsNullOrWhiteSpace(chatModel))
        {
            throw new ArgumentException("Chat model cannot be empty.", nameof(chatModel));
        }

        if (contextLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(contextLimit),
                "RAG context limit must be greater than zero.");
        }

        var normalizedQuestion = question.Trim();

        var contextResults = await _semanticSearchService.SearchAsync(
            normalizedQuestion,
            embeddingModel,
            contextLimit,
            cancellationToken);

        var relevantContextResults = contextResults
            .Where(result => !string.IsNullOrWhiteSpace(result.Text))
            .ToArray();

        if (relevantContextResults.Length == 0)
        {
            return BuildNoContextResult(normalizedQuestion, chatModel);
        }

        var ragPrompt = MemoryRagPromptBuilder.Build(
            normalizedQuestion,
            relevantContextResults,
            contextLimit);

        var chatResponse = await _chatCompletionService.CompleteAsync(
            new ChatCompletionRequest
            {
                Model = chatModel,
                Temperature = 0.2m,
                Messages =
                [
                    new ChatCompletionMessage
                    {
                        Role = ChatMessageRoles.System,
                        Content = ragPrompt.SystemPrompt
                    },
                    new ChatCompletionMessage
                    {
                        Role = ChatMessageRoles.User,
                        Content = ragPrompt.UserPrompt
                    }
                ]
            },
            cancellationToken);

        return new MemoryRagAnswerResult
        {
            Question = normalizedQuestion,
            Answer = chatResponse.Content,
            Provider = chatResponse.Provider,
            Model = chatResponse.Model,
            ContextItemsCount = ragPrompt.ContextItemsCount,
            ContextResults = relevantContextResults
        };
    }

    /// <summary>
    /// Builds a deterministic RAG answer when no indexed memory context is available.
    /// </summary>
    private static MemoryRagAnswerResult BuildNoContextResult(
        string normalizedQuestion,
        string chatModel)
    {
        return new MemoryRagAnswerResult
        {
            Question = normalizedQuestion,
            Answer = """
            No indexed memories were found for this question.

            Run:
              devmemory index

            or inspect what would be indexed with:
              devmemory index --dry-run
            """,
            Provider = AiProviderNames.None,
            Model = chatModel,
            ContextItemsCount = 0,
            ContextResults = []
        };
    }
}
