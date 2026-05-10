using DevMemory.Cli.CommandLine;
using DevMemory.Cli.Commands;

namespace DevMemory.Cli.Tests;

public sealed class CommandDispatcherTests
{
    [Fact]
    public void Dispatch_WhenNoArgsAreProvided_ExecutesHelpAndReturnsInvalidCommand()
    {
        // Arrange
        var helpHandler = new FakeCommandHandler("help", CliExitCodes.Success);

        var dispatcher = new CommandDispatcher(
        [
            helpHandler
        ]);

        // Act
        var exitCode = dispatcher.Dispatch([]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, exitCode);
        Assert.True(helpHandler.WasExecuted);
        Assert.Empty(helpHandler.ReceivedArgs);
    }

    [Fact]
    public void Dispatch_WhenKnownCommandIsProvided_ExecutesMatchingHandler()
    {
        // Arrange
        var listHandler = new FakeCommandHandler("list", CliExitCodes.Success);
        var helpHandler = new FakeCommandHandler("help", CliExitCodes.Success);

        var dispatcher = new CommandDispatcher(
        [
            listHandler,
            helpHandler
        ]);

        var args = new[]
        {
            "list"
        };

        // Act
        var exitCode = dispatcher.Dispatch(args);

        // Assert
        Assert.Equal(CliExitCodes.Success, exitCode);
        Assert.True(listHandler.WasExecuted);
        Assert.Equal(args, listHandler.ReceivedArgs);
        Assert.False(helpHandler.WasExecuted);
    }

    [Fact]
    public void Dispatch_WhenHelpAliasIsProvided_ExecutesHelpHandler()
    {
        // Arrange
        var helpHandler = new FakeCommandHandler("help", CliExitCodes.Success);

        var dispatcher = new CommandDispatcher(
        [
            helpHandler
        ]);

        var args = new[]
        {
            "--help"
        };

        // Act
        var exitCode = dispatcher.Dispatch(args);

        // Assert
        Assert.Equal(CliExitCodes.Success, exitCode);
        Assert.True(helpHandler.WasExecuted);
        Assert.Equal(args, helpHandler.ReceivedArgs);
    }

    [Fact]
    public void Dispatch_WhenUnknownCommandIsProvided_ReturnsInvalidCommand()
    {
        // Arrange
        var listHandler = new FakeCommandHandler("list", CliExitCodes.Success);
        var helpHandler = new FakeCommandHandler("help", CliExitCodes.Success);

        var dispatcher = new CommandDispatcher(
        [
            listHandler,
            helpHandler
        ]);

        // Act
        var exitCode = dispatcher.Dispatch(["unknown"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, exitCode);
        Assert.False(listHandler.WasExecuted);
        Assert.True(helpHandler.WasExecuted);
    }

    [Fact]
    public void Dispatch_WhenHandlerThrowsArgumentException_ReturnsInvalidCommand()
    {
        // Arrange
        var failingHandler = new FakeCommandHandler(
            "search",
            CliExitCodes.Success,
            new ArgumentException("Invalid option."));

        var helpHandler = new FakeCommandHandler("help", CliExitCodes.Success);

        var dispatcher = new CommandDispatcher(
        [
            failingHandler,
            helpHandler
        ]);

        // Act
        var exitCode = dispatcher.Dispatch(["search", "test", "--project"]);

        // Assert
        Assert.Equal(CliExitCodes.InvalidCommand, exitCode);
        Assert.True(failingHandler.WasExecuted);
        Assert.False(helpHandler.WasExecuted);
    }

    [Fact]
    public void Dispatch_WhenHandlerThrowsUnexpectedException_ReturnsFailure()
    {
        // Arrange
        var failingHandler = new FakeCommandHandler(
            "list",
            CliExitCodes.Success,
            new InvalidOperationException("Storage unavailable."));

        var helpHandler = new FakeCommandHandler("help", CliExitCodes.Success);

        var dispatcher = new CommandDispatcher(
        [
            failingHandler,
            helpHandler
        ]);

        // Act
        var exitCode = dispatcher.Dispatch(["list"]);

        // Assert
        Assert.Equal(CliExitCodes.Failure, exitCode);
        Assert.True(failingHandler.WasExecuted);
        Assert.False(helpHandler.WasExecuted);
    }

    private sealed class FakeCommandHandler : ICommandHandler
    {
        private readonly int _exitCode;
        private readonly Exception? _exceptionToThrow;

        public FakeCommandHandler(
            string name,
            int exitCode,
            Exception? exceptionToThrow = null)
        {
            Name = name;
            _exitCode = exitCode;
            _exceptionToThrow = exceptionToThrow;
        }

        public string Name { get; }

        public bool WasExecuted { get; private set; }

        public string[] ReceivedArgs { get; private set; } = [];

        public int Execute(string[] args)
        {
            WasExecuted = true;
            ReceivedArgs = args;

            if (_exceptionToThrow is not null)
            {
                throw _exceptionToThrow;
            }

            return _exitCode;
        }
    }
}
