using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevMemory.Application.Abstractions.Ai;
using DevMemory.Application.Models.Ai;

namespace DevMemory.Infrastructure.Ai.Qdrant;

public sealed class QdrantVectorMemoryStore : IVectorMemoryStore, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = JsonSerializerOptions.Web;

    private readonly HttpClient _httpClient;
    private readonly string _collectionName;
    private readonly bool _disposeHttpClient;

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

        var qdrantRequest = new QdrantUpsertRequest
        {
            Points =
            [
                new QdrantPoint
                {
                    Id = document.MemoryId,
                    Vector = document.Vector,
                    Payload = BuildPayload(document)
                }
            ]
        };

        using var response = await _httpClient.PutAsJsonAsync(
            $"/collections/{_collectionName}/points?wait=true",
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
            $"/collections/{_collectionName}/points/search",
            qdrantRequest,
            JsonOptions,
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

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
    /// Builds the payload persisted together with the vector.
    /// </summary>
    private static Dictionary<string, object> BuildPayload(VectorMemoryDocument document)
    {
        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["memoryId"] = document.MemoryId.ToString("D"),
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
