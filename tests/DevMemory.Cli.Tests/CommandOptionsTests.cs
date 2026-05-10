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
            "legacy",
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
        Assert.Equal("revision legacy", options.Query);
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
            "metadata",
            "docstruct"
        };

        // Act
        var options = CommandOptions.BuildSearchOptions(args);

        // Assert
        Assert.Equal("metadata docstruct", options.Query);
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
            "~/work/LogicalCommon"
        };

        // Act
        var path = CommandOptions.ReadPathOption(args);

        // Assert
        Assert.Equal("~/work/LogicalCommon", path);
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
            "graph-view",
            "--output",
            "~/devmemory-graph.html"
        };

        // Act
        var output = CommandOptions.ReadOutputOption(args);

        // Assert
        Assert.Equal("~/devmemory-graph.html", output);
    }

    [Fact]
    public void ReadOutputOption_WhenOutputValueIsMissing_ThrowsArgumentException()
    {
        // Arrange
        var args = new[]
        {
            "graph-view",
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
            "--project"
        };

        // Act
        var act = () => CommandOptions.ReadPathOption(args);

        // Assert
        var exception = Assert.Throws<ArgumentException>(act);
        Assert.Equal("Option --path requires a value.", exception.Message);
    }
}
