namespace DevMemory.Infrastructure;

public sealed class DevMemoryStorageOptions
{
    public string StorageDirectory { get; init; } = ResolveStorageDirectory();

    public string StorageFileName { get; init; } = "devmemory.json";

    public string MarkdownDirectoryName { get; init; } = "markdown";

    public string GraphDirectoryName { get; init; } = "graph";

    public string StorageFilePath => Path.Combine(StorageDirectory, StorageFileName);

    public string MarkdownDirectoryPath => Path.Combine(StorageDirectory, MarkdownDirectoryName);

    public string GraphDirectoryPath => Path.Combine(StorageDirectory, GraphDirectoryName);

    public string DefaultGraphFilePath => Path.Combine(GraphDirectoryPath, "devmemory-graph.json");

    public string DefaultGraphHtmlFilePath => Path.Combine(GraphDirectoryPath, "devmemory-graph.html");

    private static string ResolveStorageDirectory()
    {
        var configuredPath = Environment.GetEnvironmentVariable(DevMemoryEnvironmentVariables.Home);

        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return ExpandHomeDirectory(configuredPath.Trim());
        }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".devmemory");
    }

    private static string ExpandHomeDirectory(string path)
    {
        if (path == "~")
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        if (path.StartsWith("~/", StringComparison.Ordinal))
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                path[2..]);
        }

        return path;
    }
}
