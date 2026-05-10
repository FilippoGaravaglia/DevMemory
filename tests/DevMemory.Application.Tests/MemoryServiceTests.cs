using DevMemory.Application;
using DevMemory.Application.Abstractions;
using DevMemory.Application.Models;
using DevMemory.Core;

namespace DevMemory.Application.Tests;

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

        public string Export(TaskMemory memory)
        {
            ExportedMemoryId = memory.Id;
            return "/fake/path/memory.md";
        }
    }
}
