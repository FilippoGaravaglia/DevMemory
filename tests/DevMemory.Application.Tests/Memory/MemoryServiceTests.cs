using DevMemory.Application.Abstractions.Memory;
using DevMemory.Application.Models.Memory;
using DevMemory.Core;

namespace DevMemory.Application.Tests.Memory;

public sealed class MemoryServiceTests
{
    [Fact]
    public void Add_WhenMemoryIsProvided_SavesAndExportsMemory()
    {
        // Arrange
        var repository = new InMemoryRepository();
        var exporter = new InMemoryExporter();

        var service = new MemoryService(repository, exporter);

        var memory = new TaskMemory
        {
            Title = "Fix legacy revision root",
            Project = "LogicalCommon",
            Area = "Estimate Revision",
            Tags = ["dotnet", "unit-test"],
            Problem = "Legacy root was not detected correctly.",
            Solution = "Updated revision chain handling."
        };

        // Act
        var result = service.Add(memory);

        // Assert
        Assert.True(result.Success);
        Assert.Single(repository.SavedMemories);
        Assert.Equal(memory.Id, repository.SavedMemories[0].Id);
        Assert.Equal(memory.Id, exporter.ExportedMemoryId);
        Assert.Equal("/fake/path/memory.md", result.MarkdownFilePath);
    }

    [Fact]
    public void Search_WhenQueryMatchesTag_ReturnsMatchingMemory()
    {
        // Arrange
        var repository = new InMemoryRepository
        {
            Memories =
            [
                new TaskMemory
                {
                    Title = "Fix revision bug",
                    Project = "LogicalCommon",
                    Area = "Estimate",
                    Tags = ["dotnet", "revision"]
                },
                new TaskMemory
                {
                    Title = "Create markdown exporter",
                    Project = "DevMemory",
                    Area = "Infrastructure",
                    Tags = ["markdown"]
                }
            ]
        };

        var service = new MemoryService(repository, new InMemoryExporter());

        // Act
        var results = service.Search(new MemorySearchOptions
        {
            Query = "revision"
        });

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("Fix revision bug", result.Memory.Title);
        Assert.True(result.Score > 0);
    }

    [Fact]
    public void List_WhenMemoriesExist_ReturnsNewestFirst()
    {
        // Arrange
        var older = new TaskMemory
        {
            Title = "Older task",
            CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        var newer = new TaskMemory
        {
            Title = "Newer task",
            CreatedAt = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc)
        };

        var repository = new InMemoryRepository
        {
            Memories = [older, newer]
        };

        var service = new MemoryService(repository, new InMemoryExporter());

        // Act
        var results = service.List().ToList();

        // Assert
        Assert.Equal("Newer task", results[0].Title);
        Assert.Equal("Older task", results[1].Title);
    }

    [Fact]
    public void Add_WhenRequiredFieldsAreMissing_DoesNotSaveMemory()
    {
        // Arrange
        var repository = new InMemoryRepository();
        var exporter = new InMemoryExporter();

        var service = new MemoryService(repository, exporter);

        var memory = new TaskMemory();

        // Act
        var result = service.Add(memory);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
        Assert.Empty(repository.SavedMemories);
        Assert.Null(exporter.ExportedMemoryId);
    }

    [Fact]
    public void Add_WhenMemoryHasDuplicatedAndUntrimmedTags_NormalizesTags()
    {
        // Arrange
        var repository = new InMemoryRepository();
        var exporter = new InMemoryExporter();

        var service = new MemoryService(repository, exporter);

        var memory = new TaskMemory
        {
            Title = "  Test memory  ",
            Project = " DevMemory ",
            Area = " Application ",
            Tags = [" DotNet ", "dotnet", " CLI "],
            Problem = " Problem ",
            Solution = " Solution "
        };

        // Act
        var result = service.Add(memory);

        // Assert
        Assert.True(result.Success);

        var savedMemory = Assert.Single(repository.SavedMemories);

        Assert.Equal("Test memory", savedMemory.Title);
        Assert.Equal("DevMemory", savedMemory.Project);
        Assert.Equal("Application", savedMemory.Area);
        Assert.Equal(["dotnet", "cli"], savedMemory.Tags);
    }

    [Fact]
    public void Search_WhenProjectFilterIsProvided_ReturnsOnlyMatchingProject()
    {
        // Arrange
        var repository = new InMemoryRepository
        {
            Memories =
            [
                new TaskMemory
                {
                    Title = "Fix revision bug",
                    Project = "LogicalCommon",
                    Area = "Estimate",
                    Tags = ["revision"]
                },
                new TaskMemory
                {
                    Title = "Fix revision bug in demo",
                    Project = "DemoProject",
                    Area = "Estimate",
                    Tags = ["revision"]
                }
            ]
        };

        var service = new MemoryService(repository, new InMemoryExporter());

        // Act
        var results = service.Search(new MemorySearchOptions
        {
            Query = "revision",
            Project = "LogicalCommon"
        });

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("LogicalCommon", result.Memory.Project);
    }

    [Fact]
    public void Delete_WhenMemoryExists_RemovesMemoryFromRepository()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");

        var memory = new TaskMemory
        {
            Id = memoryId,
            Title = "Release v0.1.3 finalized",
            Project = "DevMemory",
            Area = "Release",
            Branch = "main",
            Tags = ["release"],
            Problem = "Finalize release.",
            Solution = "Published GitHub release.",
            LessonsLearned = "Keep release assets aligned.",
            CreatedAt = DateTime.UtcNow
        };

        var repository = new InMemoryMemoryRepository([memory]);
        var exporter = new TestMemoryExporter();

        var service = new MemoryService(repository, exporter);

        // Act
        var result = service.Delete(memoryId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.DeletedMemory);
        Assert.Equal(memoryId, result.DeletedMemory.Id);

        Assert.Empty(repository.Load());
        Assert.Contains(memoryId, exporter.DeletedMemoryIds);
    }

    [Fact]
    public void Delete_WhenMemoryDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var memoryId = Guid.Parse("7340ac82-4ed6-41b1-b790-e15edfaf39b4");

        var repository = new InMemoryMemoryRepository([]);
        var exporter = new TestMemoryExporter();

        var service = new MemoryService(repository, exporter);

        // Act
        var result = service.Delete(memoryId);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.DeletedMemory);
        Assert.Equal($"Memory not found: {memoryId}", result.Error);
        Assert.Empty(exporter.DeletedMemoryIds);
    }

    #region Helpers

    private sealed class InMemoryRepository : IMemoryRepository
    {
        public List<TaskMemory> Memories { get; set; } = [];

        public List<TaskMemory> SavedMemories { get; private set; } = [];

        public List<TaskMemory> Load()
        {
            return Memories.ToList();
        }

        public void Save(List<TaskMemory> memories)
        {
            SavedMemories = memories.ToList();
            Memories = memories.ToList();
        }

        public string GetStorageFilePath()
        {
            return "/fake/path/devmemory.json";
        }

        public string GetMarkdownDirectoryPath()
        {
            return "/fake/path/markdown";
        }
    }

    private sealed class InMemoryExporter : IMemoryExporter
    {
        public Guid? ExportedMemoryId { get; private set; }

        public void Delete(TaskMemory memory)
        {
            throw new NotImplementedException();
        }

        public string Export(TaskMemory memory)
        {
            ExportedMemoryId = memory.Id;
            return "/fake/path/memory.md";
        }
    }

    private sealed class InMemoryMemoryRepository : IMemoryRepository
    {
        private readonly List<TaskMemory> _memories;

        public InMemoryMemoryRepository(IReadOnlyCollection<TaskMemory> memories)
        {
            _memories = memories.ToList();
        }

        public List<TaskMemory> Load()
        {
            return _memories.ToList();
        }

        public void Save(List<TaskMemory> memories)
        {
            _memories.Clear();
            _memories.AddRange(memories);
        }

        public string GetStorageFilePath()
        {
            return "/tmp/devmemory.json";
        }

        public string GetMarkdownDirectoryPath()
        {
            return "/tmp/markdown";
        }
    }

    private sealed class TestMemoryExporter : IMemoryExporter
    {
        public List<Guid> DeletedMemoryIds { get; } = [];

        public string Export(TaskMemory memory)
        {
            return $"/tmp/markdown/{memory.Id:D}.md";
        }

        public void Delete(TaskMemory memory)
        {
            DeletedMemoryIds.Add(memory.Id);
        }
    }

    #endregion
}
