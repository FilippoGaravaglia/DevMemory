namespace DevMemory.Application.Models.Memory;

/// <summary>
/// Represents editable memory fields and collection changes.
/// </summary>
public sealed class EditMemoryOptions
{
    public string? Title { get; init; }

    public string? Project { get; init; }

    public string? Area { get; init; }

    public string? Branch { get; init; }

    public string? Problem { get; init; }

    public string? Solution { get; init; }

    public string? LessonsLearned { get; init; }

    public IReadOnlyCollection<string> TagsToAdd { get; init; } = [];

    public IReadOnlyCollection<string> TagsToRemove { get; init; } = [];

    public IReadOnlyCollection<string> DecisionsToAdd { get; init; } = [];

    public IReadOnlyCollection<string> DecisionsToRemove { get; init; } = [];

    public IReadOnlyCollection<string> FilesToAdd { get; init; } = [];

    public IReadOnlyCollection<string> FilesToRemove { get; init; } = [];

    public IReadOnlyCollection<string> TestsToAdd { get; init; } = [];

    public IReadOnlyCollection<string> TestsToRemove { get; init; } = [];

    public bool HasChanges =>
        !string.IsNullOrWhiteSpace(Title) ||
        !string.IsNullOrWhiteSpace(Project) ||
        !string.IsNullOrWhiteSpace(Area) ||
        Branch is not null ||
        !string.IsNullOrWhiteSpace(Problem) ||
        !string.IsNullOrWhiteSpace(Solution) ||
        !string.IsNullOrWhiteSpace(LessonsLearned) ||
        TagsToAdd.Count > 0 ||
        TagsToRemove.Count > 0 ||
        DecisionsToAdd.Count > 0 ||
        DecisionsToRemove.Count > 0 ||
        FilesToAdd.Count > 0 ||
        FilesToRemove.Count > 0 ||
        TestsToAdd.Count > 0 ||
        TestsToRemove.Count > 0;
}
