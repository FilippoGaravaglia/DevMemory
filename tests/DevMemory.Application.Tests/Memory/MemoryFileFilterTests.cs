using DevMemory.Application.Filtering;

namespace DevMemory.Application.Tests;

public sealed class MemoryFileFilterTests
{
    [Fact]
    public void Filter_WhenGeneratedFilesAreProvided_RemovesGeneratedFiles()
    {
        // Arrange
        var files = new[]
        {
            "src/DevMemory.Cli/Program.cs",
            "tests/DevMemory.Application.Tests/MemoryServiceTests.cs",
            "src/DevMemory.Cli/bin/Debug/net10.0/DevMemory.Cli.dll",
            "src/DevMemory.Cli/obj/project.assets.json",
            "artifacts/packages/DevMemory.Cli.0.1.0.nupkg",
            ".DS_Store",
            "README.md"
        };

        // Act
        var filteredFiles = MemoryFileFilter.Filter(files);

        // Assert
        Assert.Contains("src/DevMemory.Cli/Program.cs", filteredFiles);
        Assert.Contains("tests/DevMemory.Application.Tests/MemoryServiceTests.cs", filteredFiles);
        Assert.Contains("README.md", filteredFiles);

        Assert.DoesNotContain(filteredFiles, file => file.Contains("/bin/", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(filteredFiles, file => file.Contains("/obj/", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(filteredFiles, file => file.Contains("artifacts", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(filteredFiles, file => file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(filteredFiles, file => file.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(".DS_Store", filteredFiles);
    }

    [Fact]
    public void ShouldIgnore_WhenFileHasGeneratedCompoundSuffix_ReturnsTrue()
    {
        // Act
        var shouldIgnoreDepsJson = MemoryFileFilter.ShouldIgnore(
            "src/DevMemory.Cli/bin/Debug/net10.0/DevMemory.Cli.deps.json");

        var shouldIgnoreRuntimeConfigJson = MemoryFileFilter.ShouldIgnore(
            "src/DevMemory.Cli/bin/Debug/net10.0/DevMemory.Cli.runtimeconfig.json");

        // Assert
        Assert.True(shouldIgnoreDepsJson);
        Assert.True(shouldIgnoreRuntimeConfigJson);
    }
}
