using System.Text.Json;

namespace DevMemory.Infrastructure;

/// <summary>
/// Reads and writes the persistent DevMemory AI/RAG configuration file.
/// </summary>
public sealed class AiRuntimeConfigurationStore
{
    private const string ConfigFileName = "config.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string ConfigFilePath { get; }

    public AiRuntimeConfigurationStore()
        : this(ResolveDefaultConfigFilePath())
    {
    }

    public AiRuntimeConfigurationStore(string configFilePath)
    {
        if (string.IsNullOrWhiteSpace(configFilePath))
        {
            throw new ArgumentException("Configuration file path cannot be empty.", nameof(configFilePath));
        }

        ConfigFilePath = configFilePath;
    }

    /// <summary>
    /// Loads the persisted AI/RAG configuration, returning an empty configuration when the file does not exist.
    /// </summary>
    public AiRuntimeConfiguration Load()
    {
        if (!File.Exists(ConfigFilePath))
        {
            return new AiRuntimeConfiguration();
        }

        var json = File.ReadAllText(ConfigFilePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new AiRuntimeConfiguration();
        }

        return JsonSerializer.Deserialize<AiRuntimeConfiguration>(json, JsonOptions)
            ?? new AiRuntimeConfiguration();
    }

    /// <summary>
    /// Saves the persisted AI/RAG configuration using a temporary file and atomic replacement.
    /// </summary>
    public void Save(AiRuntimeConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var directoryPath = Path.GetDirectoryName(ConfigFilePath);

        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var temporaryFilePath = $"{ConfigFilePath}.tmp";
        var json = JsonSerializer.Serialize(configuration, JsonOptions);

        File.WriteAllText(temporaryFilePath, json);
        File.Move(temporaryFilePath, ConfigFilePath, overwrite: true);
    }

    /// <summary>
    /// Deletes the persisted AI/RAG configuration file when it exists.
    /// </summary>
    public void Reset()
    {
        if (File.Exists(ConfigFilePath))
        {
            File.Delete(ConfigFilePath);
        }
    }

    /// <summary>
    /// Resolves the default DevMemory persistent configuration file path.
    /// </summary>
    private static string ResolveDefaultConfigFilePath()
    {
        var configuredHome = Environment.GetEnvironmentVariable("DEVMEMORY_HOME");

        var homeDirectory = string.IsNullOrWhiteSpace(configuredHome)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".devmemory")
            : configuredHome.Trim();

        return Path.Combine(homeDirectory, ConfigFileName);
    }
}
