using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands;

namespace DevMemory.Cli.Tests;

public sealed class VersionCommandHandlerTests
{
    [Fact]
    public void Execute_PrintsDevMemoryVersion()
    {
        // Arrange
        using var output = new StringWriter();

        var handler = new VersionCommandHandler(output);

        // Act
        var exitCode = handler.Execute(["version"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, exitCode);
        Assert.StartsWith("DevMemory ", output.ToString(), StringComparison.Ordinal);
    }
}
