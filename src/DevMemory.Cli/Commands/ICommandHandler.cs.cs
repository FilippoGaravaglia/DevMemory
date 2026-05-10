namespace DevMemory.Cli.Commands;

public interface ICommandHandler
{
    string Name { get; }

    int Execute(string[] args);
}