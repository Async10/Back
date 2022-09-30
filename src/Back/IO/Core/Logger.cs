namespace Back.IO.Core;

using Back.IO.Abstractions;

public class Logger : ILogger
{
    public const string ErrorLevel = "ERROR";

    public const string InfoLevel = "INFO";

    private IOutput output;

    public Logger(IOutput output)
    {
        this.output = output;
    }

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
            this.output.WriteLine($"{level}: {message}");
        }
    }
}
