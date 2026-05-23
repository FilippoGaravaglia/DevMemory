namespace DevMemory.Cli.Tests.TestSupport;

public static class CliTestCollections
{
    public const string ConsoleOutput = "Console output collection";
}

[CollectionDefinition(CliTestCollections.ConsoleOutput, DisableParallelization = true)]
public sealed class ConsoleOutputCollectionDefinition
{
}
