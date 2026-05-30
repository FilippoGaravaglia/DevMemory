using DevMemory.Application.Abstractions.Memory;
using DevMemory.Application.Models.Memory;
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

    public DeleteMemoryResult Delete(Guid id)
    {
        var memories = _repository.Load();

        var memory = memories.FirstOrDefault(memory => memory.Id == id);

        if (memory is null)
        {
            return DeleteMemoryResult.Fail($"Memory not found: {id}");
        }

        memories.Remove(memory);

        _repository.Save(memories);

        _memoryExporter.Delete(memory);

        return DeleteMemoryResult.Ok(memory);
    }

    public EditMemoryResult Edit(
        Guid id,
        EditMemoryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.HasChanges)
        {
            return EditMemoryResult.Fail("At least one edit option must be provided.");
        }

        var memories = _repository.Load();

        var memory = memories.FirstOrDefault(memory => memory.Id == id);

        if (memory is null)
        {
            return EditMemoryResult.Fail($"Memory not found: {id}");
        }

        var originalMemory = Clone(memory);

        ApplyScalarChanges(memory, options);
        ApplyCollectionChanges(memory.Tags, options.TagsToAdd, options.TagsToRemove);
        ApplyCollectionChanges(memory.Decisions, options.DecisionsToAdd, options.DecisionsToRemove);
        ApplyCollectionChanges(memory.FilesTouched, options.FilesToAdd, options.FilesToRemove);
        ApplyCollectionChanges(memory.Tests, options.TestsToAdd, options.TestsToRemove);

        TaskMemoryNormalizer.Normalize(memory);

        var errors = TaskMemoryValidator.Validate(memory);

        if (errors.Count > 0)
        {
            return EditMemoryResult.Fail(errors);
        }

        _repository.Save(memories);

        _memoryExporter.Delete(originalMemory);
        var markdownFilePath = _memoryExporter.Export(memory);

        return EditMemoryResult.Ok(memory, markdownFilePath);
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

    /// <summary>
    /// Applies scalar field changes to a memory.
    /// </summary>
    private static void ApplyScalarChanges(
        TaskMemory memory,
        EditMemoryOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.Title))
        {
            memory.Title = options.Title;
        }

        if (!string.IsNullOrWhiteSpace(options.Project))
        {
            memory.Project = options.Project;
        }

        if (!string.IsNullOrWhiteSpace(options.Area))
        {
            memory.Area = options.Area;
        }

        if (options.Branch is not null)
        {
            memory.Branch = options.Branch;
        }

        if (!string.IsNullOrWhiteSpace(options.Problem))
        {
            memory.Problem = options.Problem;
        }

        if (!string.IsNullOrWhiteSpace(options.Solution))
        {
            memory.Solution = options.Solution;
        }

        if (!string.IsNullOrWhiteSpace(options.LessonsLearned))
        {
            memory.LessonsLearned = options.LessonsLearned;
        }
    }

    /// <summary>
    /// Applies add/remove changes to a memory collection.
    /// </summary>
    private static void ApplyCollectionChanges(
        List<string> values,
        IReadOnlyCollection<string> valuesToAdd,
        IReadOnlyCollection<string> valuesToRemove)
    {
        foreach (var valueToRemove in valuesToRemove)
        {
            values.RemoveAll(value =>
                value.Equals(valueToRemove, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var valueToAdd in valuesToAdd)
        {
            if (string.IsNullOrWhiteSpace(valueToAdd))
            {
                continue;
            }

            if (values.Any(value => value.Equals(valueToAdd, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            values.Add(valueToAdd);
        }
    }

    /// <summary>
    /// Creates a defensive copy of a memory before editing it.
    /// </summary>
    private static TaskMemory Clone(TaskMemory memory)
    {
        return new TaskMemory
        {
            Id = memory.Id,
            Title = memory.Title,
            Project = memory.Project,
            Area = memory.Area,
            Branch = memory.Branch,
            Tags = memory.Tags.ToList(),
            Problem = memory.Problem,
            Solution = memory.Solution,
            Decisions = memory.Decisions.ToList(),
            FilesTouched = memory.FilesTouched.ToList(),
            Tests = memory.Tests.ToList(),
            LessonsLearned = memory.LessonsLearned,
            CreatedAt = memory.CreatedAt
        };
    }
}
