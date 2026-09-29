static class LogLine
{
    public static string Message(string logLine)
    {
        return logLine.Substring(logLine.IndexOf(':') + 1).Trim();
    }

    public static string LogLevel(string logLine)
    {
        int start = logLine.IndexOf('[') + 1;
        int end = logLine.IndexOf(']');
        return logLine.Substring(start, end - start).ToLower();
    }

    public static string Reformat(string logLine)
    {
        return $"{Message(logLine)} ({LogLevel(logLine)})";
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine(LogLine.Message("[WARNING]:  Disk almost full\r\n"));
        Console.WriteLine(LogLine.LogLevel("[ERROR]: Invalid operation"));
        Console.WriteLine(LogLine.Reformat("[INFO]: Operation completed"));
    }
}