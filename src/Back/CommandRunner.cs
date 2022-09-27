namespace Back;

using System.Diagnostics;

public class CommandRunner
{
    private readonly ExecutionContext context;

    private readonly Logger logger;

    public CommandRunner(ExecutionContext context, Logger logger)
    {
        this.context = context;
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
