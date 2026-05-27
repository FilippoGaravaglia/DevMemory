using System.Net.Http;

namespace DevMemory.Cli.Commands.Ai;

/// <summary>
/// Prints user-friendly AI runtime error messages for local and cloud provider failures.
/// </summary>
internal static class AiRuntimeErrorPrinter
{
    public static void PrintFailure(string operationName, Exception exception)
    {
        Console.Error.WriteLine($"{operationName} failed.");
        Console.Error.WriteLine();

        if (IsConnectionFailure(exception))
        {
            PrintConnectionFailure(exception);

            return;
        }

        if (exception is TaskCanceledException)
        {
            PrintTimeoutFailure(exception);

            return;
        }

        Console.Error.WriteLine(exception.Message);
    }

    /// <summary>
    /// Determines whether the exception represents a connection failure.
    /// </summary>
    private static bool IsConnectionFailure(Exception exception)
    {
        return exception is HttpRequestException
            || exception.InnerException is HttpRequestException
            || exception.Message.Contains("Connection refused", StringComparison.OrdinalIgnoreCase)
            || exception.Message.Contains("actively refused", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Prints actionable guidance for local AI runtime connection failures.
    /// </summary>
    private static void PrintConnectionFailure(Exception exception)
    {
        Console.Error.WriteLine("The configured AI runtime service is not reachable.");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Details:");
        Console.Error.WriteLine($"  {exception.Message}");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Check the local AI runtime status with:");
        Console.Error.WriteLine("  ./scripts/dev-ai-local.sh doctor");
        Console.Error.WriteLine();
        Console.Error.WriteLine("If you are using local RAG, start the local services with:");
        Console.Error.WriteLine("  ./scripts/dev-ai-local.sh start");
        Console.Error.WriteLine();
        Console.Error.WriteLine("If Ollama is not installed or models are missing, run:");
        Console.Error.WriteLine("  ./scripts/dev-ai-local.sh pull-models");
    }

    /// <summary>
    /// Prints actionable guidance for timeout failures.
    /// </summary>
    private static void PrintTimeoutFailure(Exception exception)
    {
        Console.Error.WriteLine("The configured AI runtime service did not respond in time.");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Details:");
        Console.Error.WriteLine($"  {exception.Message}");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Check whether Ollama/Qdrant are running and responsive:");
        Console.Error.WriteLine("  ./scripts/dev-ai-local.sh doctor");
    }
}
