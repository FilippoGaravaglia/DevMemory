using System.Text.Json;
using DevMemory.Application.Abstractions;
using DevMemory.Application.Abstractions.Graph;
using DevMemory.Application.Models.Graph;
using DevMemory.Infrastructure.Storage;

namespace DevMemory.Infrastructure.Graph;

public sealed class JsonMemoryGraphExporter : IMemoryGraphExporter
{
    private readonly DevMemoryStorageOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonMemoryGraphExporter()
        : this(new DevMemoryStorageOptions())
    {
    }

    public JsonMemoryGraphExporter(DevMemoryStorageOptions options)
    {
        _options = options;
        Directory.CreateDirectory(_options.GraphDirectoryPath);
    }

    public string Export(MemoryGraph graph, string? outputPath = null)
    {
        var filePath = string.IsNullOrWhiteSpace(outputPath)
            ? _options.DefaultGraphFilePath
            : ExpandHomeDirectory(outputPath.Trim());

        var directoryPath = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var json = JsonSerializer.Serialize(graph, JsonOptions);

        File.WriteAllText(filePath, json);

        return filePath;
    }

    private static string ExpandHomeDirectory(string path)
    {
        if (path == "~")
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        if (path.StartsWith("~/", StringComparison.Ordinal))
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                path[2..]);
        }

        return path;
    }
}
