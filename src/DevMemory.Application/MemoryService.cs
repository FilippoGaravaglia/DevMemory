using DevMemory.Application.Abstractions;
using DevMemory.Application.Models;
using DevMemory.Application.Normalization;
using DevMemory.Application.Validation;
using DevMemory.Core;

namespace DevMemory.Application;

public sealed class MemoryService
{
    private readonly IMemoryRepository _repository;
    private readonly IMemoryExporter _memoryExporter;

    public MemoryService(
        IMemoryRepository repository,
        IMemoryExporter memoryExporter)
    {
        _repository = repository;
        _memoryExporter = memoryExporter;
    }

    public AddMemoryResult Add(TaskMemory memory)
    {
        TaskMemoryNormalizer.Normalize(memory);

        var errors = TaskMemoryValidator.Validate(memory);

        if (errors.Count > 0)
        {
            return AddMemoryResult.Fail(errors);
        }

        var memories = _repository.Load();

        memories.Add(memory);

        _repository.Save(memories);

        var markdownFilePath = _memoryExporter.Export(memory);

        return AddMemoryResult.Ok(markdownFilePath);
    }

    public IReadOnlyCollection<TaskMemory> List()
    {
        return _repository
            .Load()
            .OrderByDescending(memory => memory.CreatedAt)
            .ToList();
    }

    public TaskMemory? GetById(Guid id)
    {
        return _repository
            .Load()
            .FirstOrDefault(memory => memory.Id == id);
    }

    public IReadOnlyCollection<MemorySearchResult> Search(MemorySearchOptions options)
    {
        var query = options.Query.Trim();

        return _repository
            .Load()
            .Where(memory => MatchesFilter(memory.Project, options.Project))
            .Where(memory => MatchesFilter(memory.Area, options.Area))
            .Where(memory => string.IsNullOrWhiteSpace(options.Tag) ||
                             memory.Tags.Any(tag => tag.Equals(options.Tag.Trim(), StringComparison.OrdinalIgnoreCase)))
            .Select(memory => new MemorySearchResult
            {
                Memory = memory,
                Score = CalculateScore(memory, query)
            })
            .Where(result => string.IsNullOrWhiteSpace(query) || result.Score > 0)
            .OrderByDescending(result => result.Score)
            .ThenByDescending(result => result.Memory.CreatedAt)
            .ToList();
    }

    private static int CalculateScore(TaskMemory memory, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return 1;
        }

        var score = 0;

        score += Score(memory.Title, query, 10);
        score += Score(memory.Project, query, 6);
        score += Score(memory.Area, query, 6);
        score += Score(memory.Branch, query, 4);
        score += Score(memory.Problem, query, 5);
        score += Score(memory.Solution, query, 5);
        score += Score(memory.LessonsLearned, query, 3);

        score += memory.Tags.Sum(tag => Score(tag, query, 8));
        score += memory.Decisions.Sum(decision => Score(decision, query, 4));
        score += memory.FilesTouched.Sum(file => Score(file, query, 4));
        score += memory.Tests.Sum(test => Score(test, query, 3));

        return score;
    }

    private static int Score(string value, string query, int weight)
    {
        return value.Contains(query, StringComparison.OrdinalIgnoreCase)
            ? weight
            : 0;
    }

    private static bool MatchesFilter(string value, string? filter)
    {
        return string.IsNullOrWhiteSpace(filter) ||
               value.Equals(filter.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public string GetStorageFilePath()
    {
        return _repository.GetStorageFilePath();
    }

    public string GetMarkdownDirectoryPath()
    {
        return _repository.GetMarkdownDirectoryPath();
    }

    private static bool Contains(string value, string query)
    {
        return value.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}