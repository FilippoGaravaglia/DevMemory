namespace DevMemory.Application.Filtering;

public static class MemoryFileFilter
{
    private static readonly HashSet<string> IgnoredDirectorySegments = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin",
        "obj",
        ".git",
        ".vs",
        ".idea",
        "artifacts",
        "packages",
        "node_modules",
        "dist",
        "build",
        "coverage",
        "testresults",
        "test-results"
    };

    private static readonly HashSet<string> IgnoredFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".ds_store",
        "thumbs.db"
    };

    private static readonly string[] IgnoredFileExtensions =
    [
        ".dll",
        ".exe",
        ".pdb",
        ".cache",
        ".log",
        ".tmp",
        ".nupkg",
        ".snupkg",
        ".user",
        ".suo"
    ];

    private static readonly string[] IgnoredFileSuffixes =
    [
        ".deps.json",
        ".runtimeconfig.json",
        ".staticwebassets.runtime.json"
    ];

    public static List<string> Filter(IEnumerable<string> filePaths)
    {
        return filePaths
            .Where(ShouldInclude)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(filePath => filePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static bool ShouldInclude(string filePath)
    {
        return !ShouldIgnore(filePath);
    }

    public static bool ShouldIgnore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return true;
        }

        var normalizedPath = NormalizePath(filePath);

        return HasIgnoredDirectorySegment(normalizedPath) ||
               HasIgnoredFileName(normalizedPath) ||
               HasIgnoredExtension(normalizedPath) ||
               HasIgnoredSuffix(normalizedPath);
    }

    /// <summary>
    /// Normalizes path separators and trims external whitespace.
    /// </summary>
    private static string NormalizePath(string filePath)
    {
        return filePath
            .Trim()
            .Replace('\\', '/');
    }

    /// <summary>
    /// Determines whether the path contains a generated or tool-specific directory segment.
    /// </summary>
    private static bool HasIgnoredDirectorySegment(string normalizedPath)
    {
        var segments = normalizedPath.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return segments.Any(segment => IgnoredDirectorySegments.Contains(segment));
    }

    /// <summary>
    /// Determines whether the path points to an ignored operating-system or editor file.
    /// </summary>
    private static bool HasIgnoredFileName(string normalizedPath)
    {
        var fileName = Path.GetFileName(normalizedPath);

        return IgnoredFileNames.Contains(fileName);
    }

    /// <summary>
    /// Determines whether the file extension is known to be generated output.
    /// </summary>
    private static bool HasIgnoredExtension(string normalizedPath)
    {
        var extension = Path.GetExtension(normalizedPath);

        return !string.IsNullOrWhiteSpace(extension) &&
               IgnoredFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the file path ends with a generated compound suffix.
    /// </summary>
    private static bool HasIgnoredSuffix(string normalizedPath)
    {
        return IgnoredFileSuffixes.Any(suffix =>
            normalizedPath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
    }
}
