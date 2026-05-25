using System.Globalization;
using System.Net;
using DevMemory.Application.Models.Ai.Chat;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;
using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands.Ai;
using DevMemory.Cli.Tests.TestSupport;

namespace DevMemory.Cli.Tests.Commands.Ai;

[Collection(CliTestCollections.ConsoleOutput)]
public sealed class AiDoctorCommandHandlerTests
{
    [Fact]
    public void Execute_WhenAiIsNotConfigured_ReturnsFailureAndPrintsGuidance()
    {
        // Arrange
        var handler = new AiDoctorCommandHandler(
            static () => new AiRuntimeOptions(),
            static () => new HttpClient());

        // Act
        var result = ExecuteAndCaptureOutput(handler);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Contains("DevMemory AI doctor", result.Output, StringComparison.Ordinal);
        Assert.Contains("Chat", result.Output, StringComparison.Ordinal);
        Assert.Contains("Embeddings", result.Output, StringComparison.Ordinal);
        Assert.Contains("Vector store", result.Output, StringComparison.Ordinal);
        Assert.Contains("Result: configuration requires attention.", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenLocalRagIsConfiguredButEndpointsAreUnreachable_ReturnsFailure()
    {
        // Arrange
        var handler = new AiDoctorCommandHandler(
            CreateLocalRagOptions,
            static () => new HttpClient());

        // Act
        var result = ExecuteAndCaptureOutput(handler);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Contains("Ollama connectivity", result.Output, StringComparison.Ordinal);
        Assert.Contains("Qdrant connectivity", result.Output, StringComparison.Ordinal);
        Assert.Contains("Status: unreachable", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenOllamaIsReachableButModelsAreMissing_ReturnsFailureWithModelHints()
    {
        // Arrange
        var handler = new AiDoctorCommandHandler(
            CreateLocalRagOptions,
            static () => new HttpClient(new FakeHttpMessageHandler(request =>
            {
                if (request.RequestUri?.AbsolutePath == "/api/tags")
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("""{"models":[]}""")
                    };
                }

                if (request.RequestUri?.AbsolutePath == "/collections")
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("""{"result":{"collections":[]}}""")
                    };
                }

                if (request.RequestUri?.AbsolutePath == "/collections/devmemory_memories")
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("""{"result":{}}""")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            })));

        // Act
        var result = ExecuteAndCaptureOutput(handler);

        // Assert
        Assert.Equal(CliExitCodes.Failure, result.ExitCode);
        Assert.Contains("Chat model status: missing (llama3.2)", result.Output, StringComparison.Ordinal);
        Assert.Contains("Hint: run ollama pull llama3.2", result.Output, StringComparison.Ordinal);
        Assert.Contains("Embedding model status: missing (nomic-embed-text)", result.Output, StringComparison.Ordinal);
        Assert.Contains("Hint: run ollama pull nomic-embed-text", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void Execute_WhenLocalRuntimeIsReady_ReturnsSuccess()
    {
        // Arrange
        var handler = new AiDoctorCommandHandler(
            CreateLocalRagOptions,
            static () => new HttpClient(new FakeHttpMessageHandler(request =>
            {
                if (request.RequestUri?.AbsolutePath == "/api/tags")
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            """
                            {
                              "models": [
                                { "name": "llama3.2" },
                                { "name": "nomic-embed-text" }
                              ]
                            }
                            """)
                    };
                }

                if (request.RequestUri?.AbsolutePath == "/collections")
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("""{"result":{"collections":[{"name":"devmemory_memories"}]}}""")
                    };
                }

                if (request.RequestUri?.AbsolutePath == "/collections/devmemory_memories")
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("""{"result":{}}""")
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            })));

        // Act
        var result = ExecuteAndCaptureOutput(handler);

        // Assert
        Assert.Equal(CliExitCodes.Success, result.ExitCode);
        Assert.Contains("Chat model status: available (llama3.2)", result.Output, StringComparison.Ordinal);
        Assert.Contains("Embedding model status: available (nomic-embed-text)", result.Output, StringComparison.Ordinal);
        Assert.Contains("Collection status: available (devmemory_memories)", result.Output, StringComparison.Ordinal);
        Assert.Contains("Result: AI environment looks ready.", result.Output, StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates local RAG runtime options pointing to unavailable test ports.
    /// </summary>
    private static AiRuntimeOptions CreateLocalRagOptions()
    {
        return new AiRuntimeOptions
        {
            Chat = new ChatProviderOptions
            {
                Provider = AiProviderNames.Ollama,
                OllamaEndpoint = "http://localhost:65530",
                OllamaChatModel = "llama3.2"
            },
            Embedding = new EmbeddingProviderOptions
            {
                Provider = AiProviderNames.Ollama,
                OllamaEndpoint = "http://localhost:65530",
                OllamaEmbeddingModel = "nomic-embed-text"
            },
            VectorStore = new VectorStoreOptions
            {
                Provider = VectorStoreNames.Qdrant,
                QdrantEndpoint = "http://localhost:65531",
                QdrantCollection = "devmemory_memories"
            }
        };
    }

    /// <summary>
    /// Executes the handler and captures standard output.
    /// </summary>
    private static (int ExitCode, string Output) ExecuteAndCaptureOutput(
        AiDoctorCommandHandler handler)
    {
        var originalOutput = Console.Out;

        using var writer = new StringWriter(CultureInfo.InvariantCulture);

        try
        {
            Console.SetOut(writer);

            var exitCode = handler.Execute(["ai-doctor"]);

            return (exitCode, writer.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    /// <summary>
    /// Provides a simple fake HTTP message handler for CLI doctor tests.
    /// </summary>
    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory ?? throw new ArgumentNullException(nameof(responseFactory));
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_responseFactory(request));
        }
    }
}
