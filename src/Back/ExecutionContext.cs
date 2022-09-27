namespace Back;

public class ExecutionContext
{
    private readonly CommandLineArgsParser.Result args;

    public ExecutionContext(CommandLineArgsParser.Result args, Action<int> exit)
    {
        this.args = args;
        this.Exit = exit;
    }

    public string ProgramName =>
        this.args.ProgramName;

    // Exit program with exit code
    public Action<int> Exit { get; }
}
