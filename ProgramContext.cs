namespace Back;

public class ProgramContext
{
    private readonly string[] args;

    public ProgramContext(string[] args, Action<int> exit)
    {
        this.args = args;
        this.Exit = exit;
    }

    public string ProgramName =>
        this.args.Length > 0 ? this.args[0] : string.Empty;

    public Action<int> Exit { get; }
}
