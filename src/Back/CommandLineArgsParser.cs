namespace Back;

public static class CommandLineArgsParser
{
    public record Result(string ProgramName, string FilePath, bool Run, bool Quiet);

    private static readonly Dictionary<string, Func<Result, string, Result>> Reducers = new()
    {
        { "-r", (r, arg) => r with { Run = true } },
        { "--run", (r, arg) => r with { Run = true } },
        { "-q", (r, arg) => r with { Quiet = true } },
        { "--quiet", (r, arg) => r with { Quiet = true } },
    };

    public static Result Parse(string[] args)
    {
        var seed = new Result(args.First(), string.Empty, false, false);
        return args
            .Skip(1)  // skip program name
            .Aggregate<string, Result>(
                seed,
                (res, arg) =>
                {
                    var reducer = Reducers.GetValueOrDefault(arg, GetFilePathReducer);
                    return reducer(res, arg);
                });
    }

    private static Result GetFilePathReducer(Result r, string arg) =>
        r with { FilePath = arg };
}