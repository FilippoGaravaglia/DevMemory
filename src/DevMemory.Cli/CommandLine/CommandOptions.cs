using DevMemory.Application.Models.Memory;

namespace DevMemory.Cli.CommandLine;

public static class CommandOptions
{
    public static string[] NormalizeCommandAliases(string[] args)
    {
        if (args.Length == 0)
        {
            return ["help"];
        }

        if (args.Length == 1 && IsVersionAlias(args[0]))
        {
            return ["version"];
        }

        if (args.Length == 1 && IsHelpAlias(args[0]))
        {
            return ["help"];
        }

        return args;
    }

    public static MemorySearchOptions BuildSearchOptions(string[] args)
    {
        var queryParts = new List<string>();
        string? project = null;
        string? area = null;
        string? tag = null;

        for (var index = 1; index < args.Length; index++)
        {
            var value = args[index];

            if (value.Equals("--project", StringComparison.OrdinalIgnoreCase))
            {
                project = ReadOptionValue(args, ref index, "--project");
                continue;
            }

            if (value.Equals("--area", StringComparison.OrdinalIgnoreCase))
            {
                area = ReadOptionValue(args, ref index, "--area");
                continue;
            }

            if (value.Equals("--tag", StringComparison.OrdinalIgnoreCase))
            {
                tag = ReadOptionValue(args, ref index, "--tag");
                continue;
            }

            queryParts.Add(value);
        }

        return new MemorySearchOptions
        {
            Query = string.Join(' ', queryParts),
            Project = project,
            Area = area,
            Tag = tag
        };
    }

    public static string? ReadPathOption(string[] args)
    {
        for (var index = 1; index < args.Length; index++)
        {
            var value = args[index];

            if (value.Equals("--path", StringComparison.OrdinalIgnoreCase))
            {
                return ReadOptionValue(args, ref index, "--path");
            }
        }

        return null;
    }

    public static string? ReadOutputOption(string[] args)
    {
        for (var index = 1; index < args.Length; index++)
        {
            var value = args[index];

            if (value.Equals("--output", StringComparison.OrdinalIgnoreCase))
            {
                return ReadOptionValue(args, ref index, "--output");
            }
        }

        return null;
    }

    public static string ReadOptionValue(string[] args, ref int index, string optionName)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Option {optionName} requires a value.");
        }

        var value = args[++index];

        if (value.StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Option {optionName} requires a value.");
        }

        return value;
    }

    /// <summary>
    /// Determines whether the provided argument is a global version alias.
    /// </summary>
    private static bool IsVersionAlias(string value)
    {
        return value.Equals("--version", StringComparison.OrdinalIgnoreCase)
            || value.Equals("-v", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the provided argument is a global help alias.
    /// </summary>
    private static bool IsHelpAlias(string value)
    {
        return value.Equals("--help", StringComparison.OrdinalIgnoreCase)
            || value.Equals("-h", StringComparison.OrdinalIgnoreCase);
    }
}
