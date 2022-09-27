namespace Back;

public class Logger
{
    public void LogError(string message)
    {
        this.Log("ERROR", message);
    }

    public void LogInfo(string message)
    {
        this.Log("INFO", message);
    }

    private void Log(string level, string message)
    {
        Console.WriteLine($"{level}: {message}");
    }
}
