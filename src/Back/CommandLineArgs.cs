namespace Back;

public record CommandLineArgs(string ProgramName, string FilePath, bool Run, bool Quiet)
{
    private static readonly Dictionary<string, Func<CommandLineArgs, string, CommandLineArgs>> Reducers = new()
    {
        { "-r", (r, arg) => r with { Run = true } },
        { "--run", (r, arg) => r with { Run = true } },
        { "-q", (r, arg) => r with { Quiet = true } },
        { "--quiet", (r, arg) => r with { Quiet = true } },
    };

    public static CommandLineArgs Parse(string[] args)
    {
        var seed = new CommandLineArgs(args[0], string.Empty, false, false);
        return Prepare(args)
            .Aggregate<string, CommandLineArgs>(
                seed,
                (res, arg) =>
                {
                    var reducer = Reducers.GetValueOrDefault(arg, GetFilePathReducer);
                    return reducer(res, arg);
                });
    }

    private static IEnumerable<string> Prepare(IEnumerable<string> args)
    {
        return args
            .Skip(1)  // skip program name
            .SelectMany(arg =>
            {
                var res = new List<string>();
                if (arg.StartsWith("--") || !arg.StartsWith("-"))
                {
                    res.Add(arg);
                }
                else
                {
                    foreach (var opt in arg.Skip(1))
                    {
                        res.Add($"-{opt}");
                    }
                }

                return res;
            });
    }

    private static CommandLineArgs GetFilePathReducer(CommandLineArgs r, string arg) =>
        r with { FilePath = arg };
}