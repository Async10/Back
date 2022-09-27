namespace Back;

public class Logger
{
    public const string ErrorLevel = "ERROR";

    public const string InfoLevel = "INFO";

    public bool Quiet { get; set; } = false;

    public void LogError(string message)
    {
        this.Log(ErrorLevel, message);
    }

    public void LogInfo(string message)
    {
        this.Log(InfoLevel, message);
    }

    private void Log(string level, string message)
    {
        if (!this.Quiet || level == ErrorLevel)
        {
            Console.WriteLine($"{level}: {message}");
        }
    }
}
