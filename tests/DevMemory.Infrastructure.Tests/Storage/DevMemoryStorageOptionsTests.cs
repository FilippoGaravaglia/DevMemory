using DevMemory.Application.Abstractions.Memory;
using DevMemory.Application.Models;
using DevMemory.Infrastructure;
using DevMemory.Infrastructure.Storage;

namespace DevMemory.Infrastructure.Tests.Storage;

public sealed class DevMemoryStorageOptionsTests : IDisposable
{
    private readonly string? _originalHome;

    public DevMemoryStorageOptionsTests()
    {
        _originalHome = Environment.GetEnvironmentVariable(DevMemoryEnvironmentVariables.Home);
    }

    [Fact]
    public void StorageDirectory_WhenEnvironmentVariableIsNotSet_UsesDefaultUserProfileDirectory()
    {
        // Arrange
        Environment.SetEnvironmentVariable(DevMemoryEnvironmentVariables.Home, null);

        // Act
        var options = new DevMemoryStorageOptions();

        // Assert
        var expectedPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".devmemory");

        Assert.Equal(expectedPath, options.StorageDirectory);
    }

    [Fact]
    public void StorageDirectory_WhenEnvironmentVariableIsSet_UsesConfiguredDirectory()
    {
        // Arrange
        var customPath = Path.Combine(Path.GetTempPath(), $"devmemory-custom-{Guid.NewGuid():N}");
        Environment.SetEnvironmentVariable(DevMemoryEnvironmentVariables.Home, customPath);

        // Act
        var options = new DevMemoryStorageOptions();

        // Assert
        Assert.Equal(customPath, options.StorageDirectory);
    }

    [Fact]
    public void StorageDirectory_WhenEnvironmentVariableUsesHomeShortcut_ExpandsHomeDirectory()
    {
        // Arrange
        Environment.SetEnvironmentVariable(DevMemoryEnvironmentVariables.Home, "~/devmemory-test");

        // Act
        var options = new DevMemoryStorageOptions();

        // Assert
        var expectedPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "devmemory-test");

        Assert.Equal(expectedPath, options.StorageDirectory);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(DevMemoryEnvironmentVariables.Home, _originalHome);
    }
}
