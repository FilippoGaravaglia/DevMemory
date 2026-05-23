using DevMemory.Cli.CommandLine;

namespace DevMemory.Cli.Tests;

public sealed class CommandOptionsTests
{
    [Fact]
    public void BuildSearchOptions_WhenQueryAndFiltersAreProvided_ReturnsExpectedOptions()
    {
        // Arrange
        var args = new[]
        {
            "search",
            "revision",
            "chain",
            "--project",
            "LogicalCommon",
            "--area",
            "Estimate",
            "--tag",
            "dotnet"
        };

        // Act
        var options = CommandOptions.BuildSearchOptions(args);

        // Assert
        Assert.Equal("revision chain", options.Query);
        Assert.Equal("LogicalCommon", options.Project);
        Assert.Equal("Estimate", options.Area);
        Assert.Equal("dotnet", options.Tag);
    }

    [Fact]
    public void BuildSearchOptions_WhenOnlyQueryIsProvided_ReturnsQueryWithoutFilters()
    {
        // Arrange
        var args = new[]
        {
            "search",
            "mongodb",
            "repository"
        };

        // Act
        var options = CommandOptions.BuildSearchOptions(args);

        // Assert
        Assert.Equal("mongodb repository", options.Query);
        Assert.Null(options.Project);
        Assert.Null(options.Area);
        Assert.Null(options.Tag);
    }

    [Fact]
    public void ReadPathOption_WhenPathIsProvided_ReturnsPath()
    {
        // Arrange
        var args = new[]
        {
            "git-status",
            "--path",
            "/tmp/repository"
        };

        // Act
        var path = CommandOptions.ReadPathOption(args);

        // Assert
        Assert.Equal("/tmp/repository", path);
    }

    [Fact]
    public void ReadPathOption_WhenPathIsNotProvided_ReturnsNull()
    {
        // Arrange
        var args = new[]
        {
            "git-status"
        };

        // Act
        var path = CommandOptions.ReadPathOption(args);

        // Assert
        Assert.Null(path);
    }

    [Fact]
    public void ReadOutputOption_WhenOutputIsProvided_ReturnsOutputPath()
    {
        // Arrange
        var args = new[]
        {
            "graph-export",
            "--output",
            "/tmp/devmemory-graph.json"
        };

        // Act
        var output = CommandOptions.ReadOutputOption(args);

        // Assert
        Assert.Equal("/tmp/devmemory-graph.json", output);
    }

    [Fact]
    public void ReadOutputOption_WhenOutputValueIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var args = new[]
        {
            "graph-export",
            "--output"
        };

        // Act
        var act = () => CommandOptions.ReadOutputOption(args);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("Option --output requires a value.", exception.Message);
    }

    [Fact]
    public void ReadPathOption_WhenNextTokenIsAnotherOption_ThrowsArgumentException()
    {
        // Arrange
        var args = new[]
        {
            "git-status",
            "--path",
            "--output"
        };

        // Act
        var act = () => CommandOptions.ReadPathOption(args);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("Option --path requires a value.", exception.Message);
    }

    [Fact]
    public void NormalizeCommandAliases_WhenNoArgsAreProvided_ReturnsHelpCommand()
    {
        // Act
        var args = CommandOptions.NormalizeCommandAliases([]);

        // Assert
        Assert.Equal(["help"], args);
    }

    [Fact]
    public void NormalizeCommandAliases_WhenVersionLongAliasIsProvided_ReturnsVersionCommand()
    {
        // Act
        var args = CommandOptions.NormalizeCommandAliases(["--version"]);

        // Assert
        Assert.Equal(["version"], args);
    }

    [Fact]
    public void NormalizeCommandAliases_WhenVersionShortAliasIsProvided_ReturnsVersionCommand()
    {
        // Act
        var args = CommandOptions.NormalizeCommandAliases(["-v"]);

        // Assert
        Assert.Equal(["version"], args);
    }

    [Fact]
    public void NormalizeCommandAliases_WhenHelpLongAliasIsProvided_ReturnsHelpCommand()
    {
        // Act
        var args = CommandOptions.NormalizeCommandAliases(["--help"]);

        // Assert
        Assert.Equal(["help"], args);
    }

    [Fact]
    public void NormalizeCommandAliases_WhenHelpShortAliasIsProvided_ReturnsHelpCommand()
    {
        // Act
        var args = CommandOptions.NormalizeCommandAliases(["-h"]);

        // Assert
        Assert.Equal(["help"], args);
    }

    [Fact]
    public void NormalizeCommandAliases_WhenRegularCommandIsProvided_ReturnsOriginalArgs()
    {
        // Arrange
        var originalArgs = new[]
        {
            "search",
            "revision",
            "--project",
            "LogicalCommon"
        };

        // Act
        var args = CommandOptions.NormalizeCommandAliases(originalArgs);

        // Assert
        Assert.Same(originalArgs, args);
    }
}
