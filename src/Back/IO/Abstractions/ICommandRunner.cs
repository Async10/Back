namespace Back.IO.Abstractions;

public interface ICommandRunner
{
    bool Run(string command, params string[] args);
}
