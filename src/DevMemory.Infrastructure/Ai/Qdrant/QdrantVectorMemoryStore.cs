using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevMemory.Application.Abstractions;
using DevMemory.Application.Models.Ai.VectorStore;

namespace DevMemory.Infrastructure.Ai.Qdrant;

public sealed class QdrantVectorMemoryStore : IVectorMemoryStore, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = JsonSerializerOptions.Web;

    private readonly HttpClient _httpClient;
    private readonly string _collectionName;
    private readonly bool _disposeHttpClient;

    private int? _ensuredVectorSize;

    public QdrantVectorMemoryStore(string endpoint, string collectionName)
        : this(CreateHttpClient(endpoint), collectionName, disposeHttpClient: true)
    {
    }

    public QdrantVectorMemoryStore(HttpClient httpClient, string collectionName)
        : this(httpClient, collectionName, disposeHttpClient: false)
    {
    }

    private QdrantVectorMemoryStore(
        HttpClient httpClient,
        string collectionName,
        bool disposeHttpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _collectionName = NormalizeCollectionName(collectionName);
        _disposeHttpClient = disposeHttpClient;
    }

    public async Task UpsertAsync(
        VectorMemoryDocument document,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        ValidateDocument(document);

        await EnsureCollectionAsync(document.Vector.Count, cancellationToken);

        var qdrantRequest = new QdrantUpsertRequest
        {
            Points =
            [
                new QdrantPoint
                {
                    Id = BuildPointId(document),
                    Vector = document.Vector,
                    Payload = BuildPayload(document)
                }
            ]
        };

        using var response = await _httpClient.PutAsJsonAsync(
            $"{BuildCollectionPath()}/points?wait=true",
            qdrantRequest,
            JsonOptions,
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Qdrant upsert request failed with status code {(int)response.StatusCode}. Response: {responseContent}");
        }
    }

    public async Task<IReadOnlyCollection<VectorMemorySearchResult>> SearchAsync(
        IReadOnlyList<float> queryVector,
        int limit,
        CancellationToken cancellationToken)
    {
        ValidateSearch(queryVector, limit);

        var qdrantRequest = new QdrantSearchRequest
        {
            Vector = queryVector,
            Limit = limit,
            WithPayload = true
        };

        using var response = await _httpClient.PostAsJsonAsync(
            $"{BuildCollectionPath()}/points/search",
            qdrantRequest,
            JsonOptions,
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Qdrant search request failed with status code {(int)response.StatusCode}. Response: {responseContent}");
        }

        var qdrantResponse = JsonSerializer.Deserialize<QdrantSearchResponse>(
            responseContent,
            JsonOptions);

        return qdrantResponse?.Result
            .Select(MapSearchResult)
            .ToArray()
            ?? [];
    }

    public void Dispose()
    {
        if (_disposeHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    /// <summary>
    /// Ensures that the configured Qdrant collection exists before writing points.
    /// </summary>
    private async Task EnsureCollectionAsync(
        int vectorSize,
        CancellationToken cancellationToken)
    {
        if (_ensuredVectorSize == vectorSize)
        {
            return;
        }

        using var getResponse = await _httpClient.GetAsync(
            BuildCollectionPath(),
            cancellationToken);

        var getResponseContent = await getResponse.Content.ReadAsStringAsync(cancellationToken);

        if (getResponse.IsSuccessStatusCode)
        {
            ValidateExistingCollectionVectorSize(getResponseContent, vectorSize);

            _ensuredVectorSize = vectorSize;

            return;
        }

        if (getResponse.StatusCode != HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException(
                $"Qdrant collection check failed with status code {(int)getResponse.StatusCode}. Response: {getResponseContent}");
        }

        var createRequest = new QdrantCreateCollectionRequest
        {
            Vectors = new QdrantVectorConfiguration
            {
                Size = vectorSize,
                Distance = "Cosine"
            }
        };

        using var createResponse = await _httpClient.PutAsJsonAsync(
            BuildCollectionPath(),
            createRequest,
            JsonOptions,
            cancellationToken);

        var createResponseContent = await createResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!createResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Qdrant collection creation failed with status code {(int)createResponse.StatusCode}. Response: {createResponseContent}");
        }

        _ensuredVectorSize = vectorSize;
    }

    /// <summary>
    /// Validates that an existing Qdrant collection is compatible with the current embedding vector size.
    /// </summary>
    private static void ValidateExistingCollectionVectorSize(
        string responseContent,
        int expectedVectorSize)
    {
        var existingVectorSize = TryReadExistingCollectionVectorSize(responseContent);

        if (existingVectorSize is null)
        {
            return;
        }

        if (existingVectorSize.Value == expectedVectorSize)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Qdrant collection vector size mismatch. Existing size is {existingVectorSize.Value}, but current embedding size is {expectedVectorSize}. " +
            "Use the same embedding model used to create the collection, or recreate/reindex the collection.");
    }

    /// <summary>
    /// Reads the vector size from a Qdrant collection response when available.
    /// </summary>
    private static int? TryReadExistingCollectionVectorSize(string responseContent)
    {
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return null;
        }

        try
        {
            using var json = JsonDocument.Parse(responseContent);

            var root = json.RootElement;

            if (!root.TryGetProperty("result", out var result))
            {
                return null;
            }

            if (!result.TryGetProperty("config", out var config))
            {
                return null;
            }

            if (!config.TryGetProperty("params", out var parameters))
            {
                return null;
            }

            if (!parameters.TryGetProperty("vectors", out var vectors))
            {
                return null;
            }

            if (vectors.TryGetProperty("size", out var unnamedVectorSize) &&
                unnamedVectorSize.ValueKind == JsonValueKind.Number &&
                unnamedVectorSize.TryGetInt32(out var size))
            {
                return size;
            }

            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Creates the HTTP client used to communicate with Qdrant.
    /// </summary>
    private static HttpClient CreateHttpClient(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new ArgumentException("Qdrant endpoint cannot be empty.", nameof(endpoint));
        }

        return new HttpClient
        {
            BaseAddress = new Uri(endpoint, UriKind.Absolute)
        };
    }

    /// <summary>
    /// Normalizes and validates the Qdrant collection name.
    /// </summary>
    private static string NormalizeCollectionName(string collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            throw new ArgumentException("Qdrant collection name cannot be empty.", nameof(collectionName));
        }

        return collectionName.Trim();
    }

    /// <summary>
    /// Builds the configured Qdrant collection path.
    /// </summary>
    private string BuildCollectionPath()
    {
        return $"/collections/{Uri.EscapeDataString(_collectionName)}";
    }

    /// <summary>
    /// Validates a vector memory document before upserting it into Qdrant.
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

        if (document.Vector.Count == 0)
        {
            throw new ArgumentException("Vector document vector cannot be empty.", nameof(document));
        }
    }

    /// <summary>
    /// Validates vector search input before querying Qdrant.
    /// </summary>
    private static void ValidateSearch(IReadOnlyList<float> queryVector, int limit)
    {
        if (queryVector.Count == 0)
        {
            throw new ArgumentException("Search vector cannot be empty.", nameof(queryVector));
        }

        if (limit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Search limit must be greater than zero.");
        }
    }

    /// <summary>
    /// Builds a stable Qdrant point id for the vector memory document.
    /// </summary>
    private static Guid BuildPointId(VectorMemoryDocument document)
    {
        if (Guid.TryParse(document.DocumentId, out var documentId))
        {
            return documentId;
        }

        return document.MemoryId;
    }

    /// <summary>
    /// Builds the payload persisted together with the vector.
    /// </summary>
    private static Dictionary<string, object> BuildPayload(VectorMemoryDocument document)
    {
        var documentId = string.IsNullOrWhiteSpace(document.DocumentId)
            ? document.MemoryId.ToString("D")
            : document.DocumentId;

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["memoryId"] = document.MemoryId.ToString("D"),
            ["documentId"] = documentId,
            ["contentHash"] = document.ContentHash,
            ["title"] = document.Title,
            ["project"] = document.Project,
            ["area"] = document.Area,
            ["branch"] = document.Branch,
            ["tags"] = document.Tags,
            ["filesTouched"] = document.FilesTouched,
            ["text"] = document.Text
        };
    }

    /// <summary>
    /// Maps a Qdrant search result to the provider-independent vector memory search result.
    /// </summary>
    private static VectorMemorySearchResult MapSearchResult(QdrantScoredPoint point)
    {
        var payload = point.Payload ?? new Dictionary<string, JsonElement>(StringComparer.Ordinal);

        return new VectorMemorySearchResult
        {
            MemoryId = ReadGuidPayload(payload, "memoryId", point.Id),
            Title = ReadStringPayload(payload, "title"),
            Project = ReadStringPayload(payload, "project"),
            Area = ReadStringPayload(payload, "area"),
            Text = ReadStringPayload(payload, "text"),
            Score = Convert.ToDecimal(point.Score)
        };
    }

    /// <summary>
    /// Reads a GUID payload value from a Qdrant payload dictionary.
    /// </summary>
    private static Guid ReadGuidPayload(
        IReadOnlyDictionary<string, JsonElement> payload,
        string key,
        Guid fallback)
    {
        var value = ReadStringPayload(payload, key);

        return Guid.TryParse(value, out var result)
            ? result
            : fallback;
    }

    /// <summary>
    /// Reads a string payload value from a Qdrant payload dictionary.
    /// </summary>
    private static string ReadStringPayload(
        IReadOnlyDictionary<string, JsonElement> payload,
        string key)
    {
        if (!payload.TryGetValue(key, out var value))
        {
            return string.Empty;
        }

        return value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    private sealed record QdrantCreateCollectionRequest
    {
        [JsonPropertyName("vectors")]
        public QdrantVectorConfiguration Vectors { get; init; } = new();
    }

    private sealed record QdrantVectorConfiguration
    {
        [JsonPropertyName("size")]
        public int Size { get; init; }

        [JsonPropertyName("distance")]
        public string Distance { get; init; } = "Cosine";
    }

    private sealed record QdrantUpsertRequest
    {
        [JsonPropertyName("points")]
        public IReadOnlyCollection<QdrantPoint> Points { get; init; } = [];
    }

    private sealed record QdrantPoint
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("vector")]
        public IReadOnlyList<float> Vector { get; init; } = [];

        [JsonPropertyName("payload")]
        public IReadOnlyDictionary<string, object> Payload { get; init; } =
            new Dictionary<string, object>(StringComparer.Ordinal);
    }

    private sealed record QdrantSearchRequest
    {
        [JsonPropertyName("vector")]
        public IReadOnlyList<float> Vector { get; init; } = [];

        [JsonPropertyName("limit")]
        public int Limit { get; init; }

        [JsonPropertyName("with_payload")]
        public bool WithPayload { get; init; }
    }

    private sealed record QdrantSearchResponse
    {
        [JsonPropertyName("result")]
        public IReadOnlyCollection<QdrantScoredPoint> Result { get; init; } = [];
    }

    private sealed record QdrantScoredPoint
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        [JsonPropertyName("score")]
        public double Score { get; init; }

        [JsonPropertyName("payload")]
        public Dictionary<string, JsonElement>? Payload { get; init; }
    }
}
