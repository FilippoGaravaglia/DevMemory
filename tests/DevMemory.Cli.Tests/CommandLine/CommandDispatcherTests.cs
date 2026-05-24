using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands;

namespace DevMemory.Cli.Tests.CommandLine;

public sealed class CommandDispatcherTests
{
    [Fact]
    public void Dispatch_WhenNoArgsAreProvided_ExecutesHelpAndReturnsSuccess()
    {
        // Arrange
        var helpHandler = new TestCommandHandler("help", CliExitCodes.Success);
        var dispatcher = new CommandDispatcher([helpHandler]);

        // Act
        var exitCode = dispatcher.Dispatch([]);

        // Assert
        Assert.Equal(CliExitCodes.Success, exitCode);
        Assert.True(helpHandler.WasExecuted);
        Assert.Equal(["help"], helpHandler.LastArgs);
    }

    [Fact]
    public void Dispatch_WhenKnownCommandIsProvided_ExecutesMatchingHandler()
    {
        // Arrange
        var addHandler = new TestCommandHandler("add", CliExitCodes.Success);
        var helpHandler = new TestCommandHandler("help", CliExitCodes.Success);

        var dispatcher = new CommandDispatcher([addHandler, helpHandler]);

        // Act
        var exitCode = dispatcher.Dispatch(["add"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, exitCode);
        Assert.True(addHandler.WasExecuted);
        Assert.Equal(["add"], addHandler.LastArgs);
        Assert.False(helpHandler.WasExecuted);
    }

    [Fact]
    public void Dispatch_WhenHelpAliasIsProvided_ExecutesHelpHandler()
    {
        // Arrange
        var helpHandler = new TestCommandHandler("help", CliExitCodes.Success);
        var dispatcher = new CommandDispatcher([helpHandler]);

        // Act
        var exitCode = dispatcher.Dispatch(["--help"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, exitCode);
        Assert.True(helpHandler.WasExecuted);
        Assert.Equal(["help"], helpHandler.LastArgs);
    }

    [Fact]
    public void Dispatch_WhenShortHelpAliasIsProvided_ExecutesHelpHandler()
    {
        // Arrange
        var helpHandler = new TestCommandHandler("help", CliExitCodes.Success);
        var dispatcher = new CommandDispatcher([helpHandler]);

        // Act
        var exitCode = dispatcher.Dispatch(["-h"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, exitCode);
        Assert.True(helpHandler.WasExecuted);
        Assert.Equal(["help"], helpHandler.LastArgs);
    }

    [Fact]
    public void Dispatch_WhenVersionAliasIsProvided_ExecutesVersionHandler()
    {
        // Arrange
        var versionHandler = new TestCommandHandler("version", CliExitCodes.Success);
        var helpHandler = new TestCommandHandler("help", CliExitCodes.Success);

        var dispatcher = new CommandDispatcher([versionHandler, helpHandler]);

        // Act
        var exitCode = dispatcher.Dispatch(["--version"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, exitCode);
        Assert.True(versionHandler.WasExecuted);
        Assert.Equal(["version"], versionHandler.LastArgs);
        Assert.False(helpHandler.WasExecuted);
    }

    [Fact]
    public void Dispatch_WhenShortVersionAliasIsProvided_ExecutesVersionHandler()
    {
        // Arrange
        var versionHandler = new TestCommandHandler("version", CliExitCodes.Success);
        var helpHandler = new TestCommandHandler("help", CliExitCodes.Success);

        var dispatcher = new CommandDispatcher([versionHandler, helpHandler]);

        // Act
        var exitCode = dispatcher.Dispatch(["-v"]);

        // Assert
        Assert.Equal(CliExitCodes.Success, exitCode);
        Assert.True(versionHandler.WasExecuted);
        Assert.Equal(["version"], versionHandler.LastArgs);
        Assert.False(helpHandler.WasExecuted);
    }

    [Fact]
    public void Dispatch_WhenUnknownCommandIsProvided_ReturnsInvalidCommand()
    {
        // Arrange
        using var errorOutput = new StringWriter();
        var originalError = Console.Error;

        var helpHandler = new TestCommandHandler("help", CliExitCodes.Success);
        var dispatcher = new CommandDispatcher([helpHandler]);

        try
        {
            Console.SetError(errorOutput);

            // Act
            var exitCode = dispatcher.Dispatch(["unknown"]);

            // Assert
            Assert.Equal(CliExitCodes.InvalidCommand, exitCode);
            Assert.True(helpHandler.WasExecuted);
            Assert.Equal(["help"], helpHandler.LastArgs);
            Assert.Contains("Unknown command: unknown", errorOutput.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    [Fact]
    public void Dispatch_WhenHandlerThrowsArgumentException_ReturnsInvalidCommand()
    {
        // Arrange
        using var errorOutput = new StringWriter();
        var originalError = Console.Error;

        var invalidHandler = new ThrowingCommandHandler(
            "invalid",
            new ArgumentException("Invalid option."));

        var helpHandler = new TestCommandHandler("help", CliExitCodes.Success);
        var dispatcher = new CommandDispatcher([invalidHandler, helpHandler]);

        try
        {
            Console.SetError(errorOutput);

            // Act
            var exitCode = dispatcher.Dispatch(["invalid"]);

            // Assert
            Assert.Equal(CliExitCodes.InvalidCommand, exitCode);
            Assert.Contains("Invalid option.", errorOutput.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    [Fact]
    public void Dispatch_WhenHandlerThrowsUnexpectedException_ReturnsFailure()
    {
        // Arrange
        using var errorOutput = new StringWriter();
        var originalError = Console.Error;

        var failingHandler = new ThrowingCommandHandler(
            "fail",
            new InvalidOperationException("Storage unavailable."));

        var helpHandler = new TestCommandHandler("help", CliExitCodes.Success);
        var dispatcher = new CommandDispatcher([failingHandler, helpHandler]);

        try
        {
            Console.SetError(errorOutput);

            // Act
            var exitCode = dispatcher.Dispatch(["fail"]);

            // Assert
            Assert.Equal(CliExitCodes.Failure, exitCode);
            Assert.Contains("Unexpected error.", errorOutput.ToString(), StringComparison.Ordinal);
            Assert.Contains("Storage unavailable.", errorOutput.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    private sealed class TestCommandHandler : ICommandHandler
    {
        private readonly int _exitCode;

        public TestCommandHandler(string name, int exitCode)
        {
            Name = name;
            _exitCode = exitCode;
        }

        public string Name { get; }

        public bool WasExecuted { get; private set; }

        public string[] LastArgs { get; private set; } = [];

        public int Execute(string[] args)
        {
            WasExecuted = true;
            LastArgs = args;

            return _exitCode;
        }
    }

    private sealed class ThrowingCommandHandler : ICommandHandler
    {
        private readonly Exception _exception;

        public ThrowingCommandHandler(string name, Exception exception)
        {
            Name = name;
            _exception = exception;
        }

        public string Name { get; }

        public int Execute(string[] args)
        {
            throw _exception;
        }
    }
}
