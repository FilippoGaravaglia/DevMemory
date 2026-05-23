using DevMemory.Application.Models.Ai;
using DevMemory.Infrastructure.Ai;
using DevMemory.Infrastructure.Ai.Ollama;

namespace DevMemory.Infrastructure.Tests;

public sealed class ChatCompletionServiceFactoryTests
{
    [Fact]
    public void Create_WhenChatIsDisabled_ReturnsNull()
    {
        // Arrange
        var options = new AiRuntimeOptions();

        // Act
        var service = ChatCompletionServiceFactory.Create(options);

        // Assert
        Assert.Null(service);
    }

    [Fact]
    public void Create_WhenOllamaIsConfigured_ReturnsOllamaService()
    {
        // Arrange
        var options = new AiRuntimeOptions
        {
            Chat = new ChatProviderOptions
            {
                Provider = AiProviderNames.Ollama,
                OllamaEndpoint = "http://localhost:11434",
                OllamaChatModel = "llama3.2"
            }
        };

        // Act
        var service = ChatCompletionServiceFactory.Create(options);

        // Assert
        Assert.IsType<OllamaChatCompletionService>(service);

        if (service is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    [Fact]
    public void Create_WhenCloudProviderIsConfiguredButNotImplemented_ReturnsNull()
    {
        // Arrange
        var options = new AiRuntimeOptions
        {
            Chat = new ChatProviderOptions
            {
                Provider = AiProviderNames.OpenAi,
                OpenAiApiKey = "test-key",
                OpenAiChatModel = "gpt-test"
            }
        };

        // Act
        var service = ChatCompletionServiceFactory.Create(options);

        // Assert
        Assert.Null(service);
    }
}
