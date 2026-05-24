using DevMemory.Application.Abstractions;
using DevMemory.Application.Models.Ai;
using DevMemory.Application.Models.Ai.Embeddings;
using DevMemory.Application.Models.Ai.VectorStore;

namespace DevMemory.Application.Ai.Indexing;

/// <summary>
/// Indexes memory documents by generating embeddings and storing them in a vector memory store.
/// </summary>
public sealed class MemoryVectorIndexingService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorMemoryStore _vectorMemoryStore;

    public MemoryVectorIndexingService(
        IEmbeddingService embeddingService,
        IVectorMemoryStore vectorMemoryStore)
    {
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
        _vectorMemoryStore = vectorMemoryStore ?? throw new ArgumentNullException(nameof(vectorMemoryStore));
    }

    public async Task<MemoryVectorIndexingResult> IndexAsync(
        IReadOnlyCollection<VectorMemoryDocument> documents,
        string embeddingModel,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(documents);

        if (string.IsNullOrWhiteSpace(embeddingModel))
        {
            throw new ArgumentException("Embedding model cannot be empty.", nameof(embeddingModel));
        }

        var indexedDocuments = 0;
        var errors = new List<string>();

        foreach (var document in documents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                ValidateDocument(document);

                var embedding = await _embeddingService.GenerateEmbeddingAsync(
                    new EmbeddingRequest
                    {
                        Model = embeddingModel,
                        Text = document.Text
                    },
                    cancellationToken);

                var indexedDocument = document with
                {
                    Vector = embedding.Vector
                };

                await _vectorMemoryStore.UpsertAsync(indexedDocument, cancellationToken);

                indexedDocuments++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                errors.Add(BuildDocumentError(document, ex));
            }
        }

        return new MemoryVectorIndexingResult
        {
            TotalDocuments = documents.Count,
            IndexedDocuments = indexedDocuments,
            FailedDocuments = errors.Count,
            Errors = errors
        };
    }

    /// <summary>
    /// Validates the vector memory document before indexing it.
    /// </summary>
    private static void ValidateDocument(VectorMemoryDocument document)
    {
        if (document.MemoryId == Guid.Empty)
        {
            throw new ArgumentException("Vector document memory id cannot be empty.", nameof(document));
        }

        if (string.IsNullOrWhiteSpace(document.Text))
        {
            throw new ArgumentException("Vector document text cannot be empty.", nameof(document));
        }
    }

    /// <summary>
    /// Builds a stable indexing error message for a single document.
    /// </summary>
    private static string BuildDocumentError(VectorMemoryDocument document, Exception exception)
    {
        var memoryId = document.MemoryId == Guid.Empty
            ? "<empty>"
            : document.MemoryId.ToString("D");

        return $"MemoryId={memoryId}: {exception.Message}";
    }
}
