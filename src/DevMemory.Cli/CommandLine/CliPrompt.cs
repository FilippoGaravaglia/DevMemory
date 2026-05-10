namespace DevMemory.Cli.CommandLine;

public static class CliPrompt
{
    public static string AskRequired(string label)
    {
        while (true)
        {
            Console.Write($"{label}: ");
            var value = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            Console.WriteLine($"{label} is required.");
        }
    }

    public static string AskRequiredWithDefault(string label, string defaultValue)
    {
        while (true)
        {
            Console.Write($"{label} [{defaultValue}]: ");
            var value = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            if (!string.IsNullOrWhiteSpace(defaultValue))
            {
                return defaultValue.Trim();
            }

            Console.WriteLine($"{label} is required.");
        }
    }

    public static string AskOptional(string label)
    {
        Console.Write($"{label}: ");
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    public static string AskOptionalWithDefault(string label, string defaultValue)
    {
        Console.Write($"{label} [{defaultValue}]: ");
        var value = Console.ReadLine();

        return string.IsNullOrWhiteSpace(value)
            ? defaultValue.Trim()
            : value.Trim();
    }

    public static List<string> AskList(string label)
    {
        Console.Write($"{label}: ");
        var value = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    public static List<string> AskMultilineList(string label)
    {
        Console.WriteLine($"{label} - write one item per line. Leave empty line to finish.");

        var values = new List<string>();

        while (true)
        {
            Console.Write("- ");
            var value = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(value))
            {
                break;
            }

            values.Add(value.Trim());
        }

        return values;
    }

    public static List<string> AskMultilineListWithDefaults(
        string label,
        IReadOnlyCollection<string> defaultValues)
    {
        if (defaultValues.Any())
        {
            Console.WriteLine($"{label} detected from Git:");

            foreach (var value in defaultValues)
            {
                Console.WriteLine($"- {value}");
            }

            Console.Write("Use detected values? [Y/n]: ");
            var answer = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(answer) ||
                answer.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                answer.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                return defaultValues.ToList();
            }
        }

        return AskMultilineList(label);
    }
}
