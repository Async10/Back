namespace Back.IO.Core;

using System.Diagnostics;
using Back.IO.Abstractions;

public class CommandRunner : ICommandRunner
{
    private readonly ILogger logger;

    public CommandRunner(ILogger logger)
    {
        this.logger = logger;
    }

    public bool Run(string command, params string[] args)
    {
        string argsString = string.Join(' ', args);
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = argsString,
        };

        string commandWithArgs = $"{command} {argsString}";
        if (Process.Start(startInfo) is Process process)
        {
            this.logger.LogInfo($"Run '{commandWithArgs}'");
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        throw new Exception($"Can not start '{commandWithArgs}'");
    }
}
