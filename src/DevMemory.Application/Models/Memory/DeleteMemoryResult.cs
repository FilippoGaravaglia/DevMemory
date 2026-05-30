using DevMemory.Core;

namespace DevMemory.Application.Models.Memory;

/// <summary>
/// Represents the result of a memory deletion operation.
/// </summary>
public sealed class DeleteMemoryResult
{
    private DeleteMemoryResult(
        bool success,
        TaskMemory? deletedMemory,
        string? error)
    {
        Success = success;
        DeletedMemory = deletedMemory;
        Error = error;
    }

    public bool Success { get; }

    public TaskMemory? DeletedMemory { get; }

    public string? Error { get; }

    public static DeleteMemoryResult Ok(TaskMemory deletedMemory)
    {
        return new DeleteMemoryResult(true, deletedMemory, null);
    }

    public static DeleteMemoryResult Fail(string error)
    {
        return new DeleteMemoryResult(false, null, error);
    }
}
