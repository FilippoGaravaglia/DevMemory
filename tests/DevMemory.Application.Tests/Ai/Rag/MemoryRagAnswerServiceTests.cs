using DevMemory.Application.Abstractions;
using DevMemory.Application.Ai.Rag;
using DevMemory.Application.Ai.Search;
using DevMemory.Application.Models.Ai.Chat;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;

namespace DevMemory.Application.Tests.Ai.Rag;

public sealed class MemoryRagAnswerServiceTests
{
    [Fact]
    public async Task AnswerAsync_WhenRequestIsValid_SearchesContextAndCallsChatProvider()
    {
        // Arrange
        var embeddingService = new FakeEmbeddingService();
        var vectorStore = new FakeVectorMemoryStore();
        var chatService = new FakeChatCompletionService();

        var semanticSearchService = new MemorySemanticSearchService(
            embeddingService,
            vectorStore);

        var service = new MemoryRagAnswerService(
            semanticSearchService,
            chatService);

        // Act
        var result = await service.AnswerAsync(
            "How did we fix estimate revision cloning?",
            "nomic-embed-text",
            "llama3.2",
            3,
            CancellationToken.None);

        // Assert
        Assert.Equal("How did we fix estimate revision cloning?", result.Question);
        Assert.Equal("We fixed it by normalizing null collections before deep cloning.", result.Answer);
        Assert.Equal(AiProviderNames.Ollama, result.Provider);
        Assert.Equal("llama3.2", result.Model);
        Assert.Equal(1, result.ContextItemsCount);

        var embeddingRequest = Assert.Single(embeddingService.Requests);

        Assert.Equal("nomic-embed-text", embeddingRequest.Model);
        Assert.Equal("How did we fix estimate revision cloning?", embeddingRequest.Text);

        var chatRequest = Assert.Single(chatService.Requests);

        Assert.Equal("llama3.2", chatRequest.Model);
        Assert.Equal(0.2m, chatRequest.Temperature);
        Assert.Equal(2, chatRequest.Messages.Count);

        Assert.Contains(
            chatRequest.Messages,
            message => message.Role == ChatMessageRoles.System &&
                       message.Content.Contains("You are DevMemory", StringComparison.Ordinal));

        Assert.Contains(
            chatRequest.Messages,
            message => message.Role == ChatMessageRoles.User &&
                       message.Content.Contains("Retrieved memory context:", StringComparison.Ordinal) &&
                       message.Content.Contains("Estimate revision cloning", StringComparison.Ordinal) &&
                       message.Content.Contains("normalizing null collections", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AnswerAsync_WhenQuestionIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.AnswerAsync(string.Empty, "embed", "chat", 3, CancellationToken.None));

        // Assert
        Assert.Contains("RAG question cannot be empty.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnswerAsync_WhenEmbeddingModelIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.AnswerAsync("question", string.Empty, "chat", 3, CancellationToken.None));

        // Assert
        Assert.Contains("Embedding model cannot be empty.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnswerAsync_WhenChatModelIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.AnswerAsync("question", "embed", string.Empty, 3, CancellationToken.None));

        // Assert
        Assert.Contains("Chat model cannot be empty.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnswerAsync_WhenContextLimitIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.AnswerAsync("question", "embed", "chat", 0, CancellationToken.None));

        // Assert
        Assert.Contains("RAG context limit must be greater than zero.", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates a RAG answer service with fake dependencies.
    /// </summary>
    private static MemoryRagAnswerService CreateService()
    {
        return new MemoryRagAnswerService(
            new MemorySemanticSearchService(
                new FakeEmbeddingService(),
                new FakeVectorMemoryStore()),
            new FakeChatCompletionService());
    }

    private sealed class FakeEmbeddingService : IEmbeddingService
    {
        public List<EmbeddingRequest> Requests { get; } = [];

        public Task<EmbeddingResponse> GenerateEmbeddingAsync(
            EmbeddingRequest request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);

            return Task.FromResult(new EmbeddingResponse
            {
                Provider = AiProviderNames.Ollama,
                Model = request.Model,
                Vector = [0.1f, 0.2f, 0.3f]
            });
        }
    }

    private sealed class FakeVectorMemoryStore : IVectorMemoryStore
    {
        public Task UpsertAsync(
            VectorMemoryDocument document,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyCollection<VectorMemorySearchResult>> SearchAsync(
            IReadOnlyList<float> queryVector,
            int limit,
            CancellationToken cancellationToken)
        {
            IReadOnlyCollection<VectorMemorySearchResult> results =
            [
                new VectorMemorySearchResult
                {
                    MemoryId = Guid.NewGuid(),
                    Title = "Estimate revision cloning",
                    Project = "LogicalCommon",
                    Area = "Estimate",
                    Text = "We fixed revision cloning by normalizing null collections before deep cloning.",
                    Score = 0.91m
                }
            ];

            return Task.FromResult(results);
        }
    }

    private sealed class FakeChatCompletionService : IChatCompletionService
    {
        public List<ChatCompletionRequest> Requests { get; } = [];

        public Task<ChatCompletionResponse> CompleteAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);

            return Task.FromResult(new ChatCompletionResponse
            {
                Provider = AiProviderNames.Ollama,
                Model = request.Model,
                Content = "We fixed it by normalizing null collections before deep cloning."
            });
        }
    }
}
