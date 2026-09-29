enum LogLevel
{
    Unknown = 0,
    Trace = 1,
    Debug = 2,
    Info = 4,
    Warning = 5,
    Error = 6,
    Fatal = 42
}

static class LogLine
{
    public static LogLevel ParseLogLevel(string logLine)
    {
        string level = logLine.Substring(1, 3);

        switch (level)
        {
            case "TRC": return LogLevel.Trace;
            case "DBG": return LogLevel.Debug;
            case "INF": return LogLevel.Info;
            case "WRN": return LogLevel.Warning;
            case "ERR": return LogLevel.Error;
            case "FTL": return LogLevel.Fatal;
            default: return LogLevel.Unknown;
        }
    }

    public static string OutputForShortLog(LogLevel logLevel, string message)
    {
        return $"{(int)logLevel}:{message}";
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine(LogLine.ParseLogLevel("[INF]: File deleted"));                    // Info
        Console.WriteLine(LogLine.ParseLogLevel("[XYZ]: Overly specific, out of context"));  // Unknown
        Console.WriteLine(LogLine.OutputForShortLog(LogLevel.Error, "Stack overflow"));      // 6:Stack overflow
    }
}