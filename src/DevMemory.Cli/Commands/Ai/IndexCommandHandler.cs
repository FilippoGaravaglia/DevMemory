using System.Globalization;
using DevMemory.Application;
using DevMemory.Application.Abstractions;
using DevMemory.Application.Ai.Indexing;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.Runtime;
using DevMemory.Application.Models.Ai.VectorStore;
using DevMemory.Cli.CommandLine;
using DevMemory.Infrastructure;
using DevMemory.Infrastructure.Ai.Factories;

namespace DevMemory.Cli.Commands.Ai;

public sealed class IndexCommandHandler : ICommandHandler
{
    private const int DryRunPreviewLimit = 10;

    private readonly Func<AiRuntimeOptions> _optionsFactory;
    private readonly Func<AiRuntimeOptions, IEmbeddingService?> _embeddingServiceFactory;
    private readonly Func<AiRuntimeOptions, IVectorMemoryStore?> _vectorMemoryStoreFactory;
    private readonly Func<IReadOnlyCollection<VectorMemoryDocument>> _documentsFactory;
    private readonly Func<IEmbeddingService, IVectorMemoryStore, MemoryVectorIndexingService> _indexingServiceFactory;

    public IndexCommandHandler(MemoryService memoryService)
        : this(
            AiRuntimeOptionsProvider.GetOptions,
            EmbeddingServiceFactory.Create,
            VectorMemoryStoreFactory.Create,
            () => VectorMemoryDocumentBuilder.BuildFromMemories(memoryService.List()),
            static (embeddingService, vectorMemoryStore) =>
                new MemoryVectorIndexingService(embeddingService, vectorMemoryStore))
    {
        ArgumentNullException.ThrowIfNull(memoryService);
    }

    public IndexCommandHandler(
        Func<AiRuntimeOptions> optionsFactory,
        Func<AiRuntimeOptions, IEmbeddingService?> embeddingServiceFactory,
        Func<AiRuntimeOptions, IVectorMemoryStore?> vectorMemoryStoreFactory,
        Func<IReadOnlyCollection<VectorMemoryDocument>> documentsFactory,
        Func<IEmbeddingService, IVectorMemoryStore, MemoryVectorIndexingService> indexingServiceFactory)
    {
        _optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
        _embeddingServiceFactory = embeddingServiceFactory
            ?? throw new ArgumentNullException(nameof(embeddingServiceFactory));
        _vectorMemoryStoreFactory = vectorMemoryStoreFactory
            ?? throw new ArgumentNullException(nameof(vectorMemoryStoreFactory));
        _documentsFactory = documentsFactory ?? throw new ArgumentNullException(nameof(documentsFactory));
        _indexingServiceFactory = indexingServiceFactory
            ?? throw new ArgumentNullException(nameof(indexingServiceFactory));
    }

    public string Name => "index";

    public int Execute(string[] args)
    {
        var request = ParseRequest(args);

        if (request.IsDryRun)
        {
            return ExecuteDryRun(request);
        }

        return ExecuteIndex(request);
    }

    /// <summary>
    /// Executes a dry-run indexing operation without calling embeddings or vector store adapters.
    /// </summary>
    private int ExecuteDryRun(IndexCommandRequest request)
    {
        try
        {
            var documents = ApplyLimit(
                ApplyFilters(_documentsFactory(), request),
                request.Limit);

            PrintDryRunResult(documents, request);

            return CliExitCodes.Success;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            AiRuntimeErrorPrinter.PrintFailure("Memory vector index dry-run", ex);

            return CliExitCodes.Failure;
        }
    }

    /// <summary>
    /// Executes the real indexing operation by generating embeddings and upserting documents.
    /// </summary>
    private int ExecuteIndex(IndexCommandRequest request)
    {
        var options = _optionsFactory();

        if (!options.IsSemanticSearchEnabled)
        {
            PrintSemanticSearchNotConfigured();

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
            var documents = ApplyLimit(
                ApplyFilters(_documentsFactory(), request),
                request.Limit);

            var embeddingModel = ResolveEmbeddingModel(options.Embedding);

            var indexingService = _indexingServiceFactory(embeddingService, vectorMemoryStore);

            var result = indexingService
                .IndexAsync(documents, embeddingModel, request.Force, CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            PrintIndexingResult(options, result, request);

            return result.FailedDocuments == 0
                ? CliExitCodes.Success
                : CliExitCodes.Failure;
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            Console.Error.WriteLine("Memory vector indexing failed.");
            Console.Error.WriteLine(ex.Message);

            return CliExitCodes.Failure;
        }
        finally
        {
            DisposeIfRequired(embeddingService);
            DisposeIfRequired(vectorMemoryStore);
        }
    }

    /// <summary>
    /// Parses index command arguments.
    /// </summary>
    private static IndexCommandRequest ParseRequest(string[] args)
    {
        var isDryRun = false;
        var force = false;
        int? limit = null;
        string? project = null;
        string? area = null;
        string? tag = null;
        var showText = false;

        for (var index = 1; index < args.Length; index++)
        {
            var value = args[index];

            if (value.Equals("--dry-run", StringComparison.OrdinalIgnoreCase))
            {
                isDryRun = true;
                continue;
            }

            if (value.Equals("--force", StringComparison.OrdinalIgnoreCase))
            {
                force = true;
                continue;
            }

            if (value.Equals("--show-text", StringComparison.OrdinalIgnoreCase))
            {
                showText = true;
                continue;
            }

            if (value.Equals("--limit", StringComparison.OrdinalIgnoreCase))
            {
                if (index + 1 >= args.Length ||
                    !int.TryParse(args[index + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedLimit) ||
                    parsedLimit <= 0)
                {
                    throw new ArgumentException("Option --limit must be a positive integer.");
                }

                limit = parsedLimit;
                index++;

                continue;
            }

            if (value.Equals("--project", StringComparison.OrdinalIgnoreCase))
            {
                project = ReadRequiredOptionValue(args, ref index, "--project");
                continue;
            }

            if (value.Equals("--area", StringComparison.OrdinalIgnoreCase))
            {
                area = ReadRequiredOptionValue(args, ref index, "--area");
                continue;
            }

            if (value.Equals("--tag", StringComparison.OrdinalIgnoreCase))
            {
                tag = ReadRequiredOptionValue(args, ref index, "--tag");
                continue;
            }

            throw new ArgumentException(
                "Usage: devmemory index [--dry-run] [--force] [--limit <number>] [--project <project>] [--area <area>] [--tag <tag>] [--show-text]");
        }

        if (isDryRun && force)
        {
            throw new ArgumentException("Options --dry-run and --force cannot be used together.");
        }

        if (showText && !isDryRun)
        {
            throw new ArgumentException("Option --show-text can only be used together with --dry-run.");
        }

        return new IndexCommandRequest(
            isDryRun,
            force,
            limit,
            project,
            area,
            tag,
            showText);
    }

    /// <summary>
    /// Reads a required option value from the command arguments.
    /// </summary>
    private static string ReadRequiredOptionValue(
        string[] args,
        ref int index,
        string optionName)
    {
        if (index + 1 >= args.Length ||
            string.IsNullOrWhiteSpace(args[index + 1]) ||
            args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Option {optionName} requires a value.");
        }

        index++;

        return args[index].Trim();
    }

    /// <summary>
    /// Applies optional metadata filters to the indexing input.
    /// </summary>
    private static VectorMemoryDocument[] ApplyFilters(
        IReadOnlyCollection<VectorMemoryDocument> documents,
        IndexCommandRequest request)
    {
        IEnumerable<VectorMemoryDocument> filteredDocuments = documents;

        if (!string.IsNullOrWhiteSpace(request.Project))
        {
            filteredDocuments = filteredDocuments.Where(document =>
                string.Equals(document.Project, request.Project, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.Area))
        {
            filteredDocuments = filteredDocuments.Where(document =>
                string.Equals(document.Area, request.Area, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            filteredDocuments = filteredDocuments.Where(document =>
                document.Tags.Any(tag =>
                    string.Equals(tag, request.Tag, StringComparison.OrdinalIgnoreCase)));
        }

        return filteredDocuments.ToArray();
    }

    /// <summary>
    /// Applies an optional document limit to the indexing input.
    /// </summary>
    private static IReadOnlyCollection<VectorMemoryDocument> ApplyLimit(
        IReadOnlyCollection<VectorMemoryDocument> documents,
        int? limit)
    {
        if (limit is null)
        {
            return documents;
        }

        return documents
            .Take(limit.Value)
            .ToArray();
    }

    /// <summary>
    /// Prints the memory vector indexing result.
    /// </summary>
    private static void PrintIndexingResult(
        AiRuntimeOptions options,
        MemoryVectorIndexingResult result,
        IndexCommandRequest request)
    {
        Console.WriteLine("DevMemory vector index");
        Console.WriteLine("----------------------");
        Console.WriteLine();
        Console.WriteLine($"Embedding provider: {options.Embedding.Provider}");
        Console.WriteLine($"Embedding model: {ResolveEmbeddingModel(options.Embedding)}");
        Console.WriteLine($"Vector store: {options.VectorStore.Provider}");
        Console.WriteLine($"Qdrant endpoint: {FormatOptional(options.VectorStore.QdrantEndpoint)}");
        Console.WriteLine($"Qdrant collection: {FormatOptional(options.VectorStore.QdrantCollection)}");
        Console.WriteLine($"Force indexing: {FormatBoolean(request.Force)}");
        Console.WriteLine($"Limit: {FormatOptionalNumber(request.Limit)}");
        Console.WriteLine($"Project filter: {FormatOptional(request.Project)}");
        Console.WriteLine($"Area filter: {FormatOptional(request.Area)}");
        Console.WriteLine($"Tag filter: {FormatOptional(request.Tag)}");
        Console.WriteLine($"Show text: {FormatBoolean(request.ShowText)}");
        Console.WriteLine();
        Console.WriteLine($"Total documents: {result.TotalDocuments}");
        Console.WriteLine($"Indexed documents: {result.IndexedDocuments}");
        Console.WriteLine($"Skipped documents: {result.SkippedDocuments}");
        Console.WriteLine($"Failed documents: {result.FailedDocuments}");

        PrintIndexingGuidance(result, request.Force);

        if (result.Errors.Count == 0)
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Errors:");

        foreach (var error in result.Errors)
        {
            Console.WriteLine($"- {error}");
        }
    }

    /// <summary>
    /// Prints a dry-run summary for documents that would be indexed.
    /// </summary>
    private static void PrintDryRunResult(
        IReadOnlyCollection<VectorMemoryDocument> documents,
        IndexCommandRequest request)
    {
        Console.WriteLine("DevMemory vector index dry-run");
        Console.WriteLine("------------------------------");
        Console.WriteLine();
        Console.WriteLine("No embeddings will be generated.");
        Console.WriteLine("No vector store writes will be performed.");
        Console.WriteLine();
        Console.WriteLine($"Limit: {FormatOptionalNumber(request.Limit)}");
        Console.WriteLine($"Project filter: {FormatOptional(request.Project)}");
        Console.WriteLine($"Area filter: {FormatOptional(request.Area)}");
        Console.WriteLine($"Tag filter: {FormatOptional(request.Tag)}");
        Console.WriteLine($"Show text: {FormatBoolean(request.ShowText)}");
        Console.WriteLine($"Total documents: {documents.Count}");

        if (documents.Count == 0)
        {
            Console.WriteLine();
            Console.WriteLine("No memories available for indexing.");

            return;
        }

        Console.WriteLine();
        Console.WriteLine("Documents preview:");

        foreach (var document in documents.Take(DryRunPreviewLimit))
        {
            PrintDryRunDocument(document, request.ShowText);
        }

        if (documents.Count > DryRunPreviewLimit)
        {
            Console.WriteLine();
            Console.WriteLine($"... {documents.Count - DryRunPreviewLimit} more document(s) not shown.");
        }
    }

    /// <summary>
    /// Prints a single vector document preview for dry-run output.
    /// </summary>
    private static void PrintDryRunDocument(
        VectorMemoryDocument document,
        bool showText)
    {
        Console.WriteLine($"- {document.Title}");
        Console.WriteLine($"  MemoryId: {document.MemoryId:D}");
        Console.WriteLine($"  DocumentId: {FormatOptional(document.DocumentId)}");
        Console.WriteLine($"  Content hash: {FormatOptional(document.ContentHash)}");
        Console.WriteLine($"  Project: {FormatOptional(document.Project)}");
        Console.WriteLine($"  Area: {FormatOptional(document.Area)}");
        Console.WriteLine($"  Branch: {FormatOptional(document.Branch)}");
        Console.WriteLine($"  Tags: {FormatCollection(document.Tags)}");
        Console.WriteLine($"  Files touched: {document.FilesTouched.Count}");
        Console.WriteLine($"  Text length: {document.Text.Length}");

        if (!showText)
        {
            return;
        }

        Console.WriteLine("  Text:");
        PrintIndentedText(document.Text, indentation: "    ");
    }

    /// <summary>
    /// Prints multiline text using the provided indentation.
    /// </summary>
    private static void PrintIndentedText(string text, string indentation)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Console.WriteLine($"{indentation}-");

            return;
        }

        using var reader = new StringReader(text);

        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            Console.WriteLine($"{indentation}{line}");
        }
    }

    /// <summary>
    /// Prints a user-friendly message when semantic search is not configured.
    /// </summary>
    private static void PrintSemanticSearchNotConfigured()
    {
        Console.Error.WriteLine("Semantic search is not configured.");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Configure both an embedding provider and a vector store before indexing.");
        Console.Error.WriteLine("Example:");
        Console.Error.WriteLine("  DEVMEMORY_EMBEDDING_PROVIDER=ollama DEVMEMORY_VECTOR_STORE=qdrant devmemory ai-status");
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

        return "-";
    }

    /// <summary>
    /// Formats an optional string value for CLI output.
    /// </summary>
    private static string FormatOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    /// <summary>
    /// Formats a boolean value for CLI output.
    /// </summary>
    private static string FormatBoolean(bool value)
    {
        return value ? "yes" : "no";
    }

    /// <summary>
    /// Formats an optional numeric value for CLI output.
    /// </summary>
    private static string FormatOptionalNumber(int? value)
    {
        return value?.ToString(CultureInfo.InvariantCulture) ?? "-";
    }

    /// <summary>
    /// Formats a collection value for CLI output.
    /// </summary>
    private static string FormatCollection(IReadOnlyCollection<string> values)
    {
        return values.Count == 0
            ? "-"
            : string.Join(", ", values);
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

    /// <summary>
    /// Prints actionable guidance based on the indexing result.
    /// </summary>
    private static void PrintIndexingGuidance(
        MemoryVectorIndexingResult result,
        bool force)
    {
        if (result.TotalDocuments == 0)
        {
            Console.WriteLine();
            Console.WriteLine("No memories are available for indexing.");
            Console.WriteLine();
            Console.WriteLine("Create a memory with:");
            Console.WriteLine("  devmemory add");

            return;
        }

        if (force)
        {
            return;
        }

        if (result.FailedDocuments == 0 &&
            result.IndexedDocuments == 0 &&
            result.SkippedDocuments == result.TotalDocuments)
        {
            Console.WriteLine();
            Console.WriteLine("No changes detected. All documents were already indexed.");
            Console.WriteLine();
            Console.WriteLine("Use:");
            Console.WriteLine("  devmemory index --force");
            Console.WriteLine();
            Console.WriteLine("to rebuild embeddings and overwrite the vector index.");
        }
    }

    private sealed record IndexCommandRequest(
        bool IsDryRun,
        bool Force,
        int? Limit,
        string? Project,
        string? Area,
        string? Tag,
        bool ShowText);
}
