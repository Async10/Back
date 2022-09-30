namespace Back.IO.Abstractions;

public interface ILogger
{
    bool Quiet { get; set; }

    void LogError(string message);

    void LogInfo(string message);
}
