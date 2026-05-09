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
        if (!File.Exists(_options.StorageFilePath))
        {
            return [];
        }

        var json = File.ReadAllText(_options.StorageFilePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<TaskMemory>>(json, JsonOptions) ?? [];
    }

    public void Save(List<TaskMemory> memories)
    {
        EnsureStorageExists();

        var json = JsonSerializer.Serialize(memories, JsonOptions);
        File.WriteAllText(_options.StorageFilePath, json);
    }

    public string GetStorageFilePath()
    {
        return _options.StorageFilePath;
    }

    private void EnsureStorageExists()
    {
        Directory.CreateDirectory(_options.StorageDirectory);

        if (!File.Exists(_options.StorageFilePath))
        {
            File.WriteAllText(_options.StorageFilePath, "[]");
        }
    }
}