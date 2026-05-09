using System.Text.Json;
using AiAgent.Core;

namespace AiAgent.Infrastructure;

public class MemoryRepository
{
    private const string FilePath = "memory.json";

    public List<MemoryEntry> Load()
    {
        if (!File.Exists(FilePath))
        {
            return [];
        }

        var json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<List<MemoryEntry>>(json) ?? [];
    }

    public void Save(List<MemoryEntry> entries)
    {
        var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(FilePath, json);
    }
}