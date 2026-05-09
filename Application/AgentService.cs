using AiAgent.Core;
using AiAgent.Infrastructure;

namespace AiAgent.Application;

public class AgentService
{
    private readonly MemoryRepository _repository;

    public AgentService()
    {
        _repository = new MemoryRepository();
    }

    public void AddMemory(string text)
    {
        var entries = _repository.Load();

        entries.Add(new MemoryEntry
        {
            Content = text
        });

        _repository.Save(entries);
    }

    public void ShowMemory()
    {
        var entries = _repository.Load();

        foreach (var entry in entries)
        {
            Console.WriteLine($"[{entry.CreatedAt:u}] {entry.Content}");
        }
    }
}