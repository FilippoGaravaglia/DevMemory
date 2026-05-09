using System.Text.Json;
using AiAgent.Core;

namespace AiAgent.Infrastructure;

public sealed class MemoryRepository
{
    private readonly DevMemoryStorageOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public MemoryRepository()
        : this(new DevMemoryStorageOptions())
    {
    }

    public MemoryRepository(DevMemoryStorageOptions options)
    {
        _options = options;
        EnsureStorageExists();
    }

    public List<TaskMemory> Load()
    {
        EnsureStorageExists();

        var json = File.ReadAllText(_options.StorageFilePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<TaskMemory>>(json, JsonOptions) ?? [];
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"The DevMemory storage file is corrupted or invalid: {_options.StorageFilePath}",
                ex);
        }
    }

    public void Save(List<TaskMemory> memories)
    {
        EnsureStorageExists();

        var json = JsonSerializer.Serialize(memories, JsonOptions);

        var tempFilePath = $"{_options.StorageFilePath}.tmp";
        var backupFilePath = $"{_options.StorageFilePath}.bak";

        File.WriteAllText(tempFilePath, json);

        if (File.Exists(_options.StorageFilePath))
        {
            File.Copy(_options.StorageFilePath, backupFilePath, overwrite: true);
        }

        File.Move(tempFilePath, _options.StorageFilePath, overwrite: true);
    }

    public string GetStorageFilePath()
    {
        return _options.StorageFilePath;
    }

    public string GetMarkdownDirectoryPath()
    {
        return _options.MarkdownDirectoryPath;
    }

    private void EnsureStorageExists()
    {
        Directory.CreateDirectory(_options.StorageDirectory);
        Directory.CreateDirectory(_options.MarkdownDirectoryPath);

        if (!File.Exists(_options.StorageFilePath))
        {
            File.WriteAllText(_options.StorageFilePath, "[]");
        }
    }
}