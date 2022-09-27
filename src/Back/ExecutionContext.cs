namespace Back;

public class ExecutionContext
{
    private readonly string[] args;

    public ExecutionContext(string[] args, Action<int> exit)
    {
        this.args = args;
        this.Exit = exit;
    }

    public string ProgramName =>
        this.args.Length > 0 ? this.args[0] : string.Empty;

    // Exit program with exit code
    public Action<int> Exit { get; }
}
