using DevMemory.Application.Abstractions.Ai;
using DevMemory.Application.Models.Ai;

namespace DevMemory.Application.Ai;

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

        var ragPrompt = MemoryRagPromptBuilder.Build(
            normalizedQuestion,
            contextResults,
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
            ContextResults = contextResults
        };
    }
}
