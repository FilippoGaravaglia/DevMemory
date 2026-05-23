using DevMemory.Core;
using DevMemory.Infrastructure;

namespace DevMemory.Infrastructure.Tests;

public sealed class MemoryRepositoryTests
{
    [Fact]
    public void Save_WhenMemoryIsProvided_CreatesStorageFile()
    {
        // Arrange
        using var tempDirectory = new TemporaryDirectory();

        var options = new DevMemoryStorageOptions
        {
            StorageDirectory = tempDirectory.Path
        };

        var repository = new MemoryRepository(options);

        var memories = new List<TaskMemory>
        {
            new()
            {
                Title = "Test memory",
                Project = "DevMemory",
                Area = "Storage"
            }
        };

        // Act
        repository.Save(memories);

        // Assert
        Assert.True(File.Exists(options.StorageFilePath));

        var loaded = repository.Load();
        var memory = Assert.Single(loaded);

        Assert.Equal("Test memory", memory.Title);
        Assert.Equal("DevMemory", memory.Project);
        Assert.Equal("Storage", memory.Area);
    }

    [Fact]
    public void Save_WhenStorageAlreadyExists_CreatesBackupFile()
    {
        // Arrange
        using var tempDirectory = new TemporaryDirectory();

        var options = new DevMemoryStorageOptions
        {
            StorageDirectory = tempDirectory.Path
        };

        var repository = new MemoryRepository(options);

        repository.Save(
        [
            new TaskMemory
            {
                Title = "First memory"
            }
        ]);

        // Act
        repository.Save(
        [
            new TaskMemory
            {
                Title = "Second memory"
            }
        ]);

        // Assert
        var backupFilePath = $"{options.StorageFilePath}.bak";

        Assert.True(File.Exists(options.StorageFilePath));
        Assert.True(File.Exists(backupFilePath));
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"devmemory-tests-{Guid.NewGuid():N}");

            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
