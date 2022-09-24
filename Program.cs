namespace Back;

using System.Diagnostics;
using System.Linq;
using System.Text;

public record Location(string Path, int Row, int Col)
{
    public override string ToString() =>
        $"({this.Path}:{this.Row}:{this.Col})";
}

public record Token(string Value, Location Location);

public class Lexer
{
    private readonly ProgramContext context;

    private readonly UsagePrinter usagePrinter;

    private readonly Logger logger;

    public Lexer(ProgramContext context, UsagePrinter usagePrinter, Logger logger)
    {
        this.context = context;
        this.usagePrinter = usagePrinter;
        this.logger = logger;
    }

    public IEnumerable<Token> LexFile(string path)
    {
        if (!File.Exists(path))
        {
            this.logger.LogError($"file {path} does not exist");
            this.context.Exit(1);
        }

        foreach ((int row, string line) in File.ReadLines(path).Enumerate())
        {
            foreach ((int col, string value) in this.LexLine(line))
                yield return new Token(value, new Location(path, row + 1, col + 1));
        }
    }

    private IEnumerable<(int, string)> LexLine(string line)
    {
        int start = 0;
        int col = this.FindNonWhiteSpace(line.Substring(start));
        while (col != -1)
        {
            int end = this.FindWhiteSpace(line.Substring(start));
            int length = end != -1 ? end - col : line.Substring(start).Length - col;
            yield return (start + col, line.Substring(start + col, length));
            start += col + length + 1;
            col = this.FindNonWhiteSpace(start < line.Length ? line.Substring(start): string.Empty);
        }
    }

    private int FindNonWhiteSpace(string text) =>
        this.FindIndex(text, ch => !Char.IsWhiteSpace(ch));

    private int FindWhiteSpace(string text) =>
        this.FindIndex(text, Char.IsWhiteSpace);

    private int FindIndex(string text, Func<char, bool> predicate)
    {
        for (int idx = 0; idx < text.Length; idx++)
        {
            if (predicate(text[idx]))
                return idx;
        }

        return -1;
    }
}

public enum Opcode
{
    PUSH,
    PLUS,
    DUMP,
}

public record Operation(Opcode Code, Location location, int? Value = null);

public class Parser
{
    public IEnumerable<Operation> Parse(IEnumerable<Token> tokens)
    {
        foreach (var token in tokens)
            yield return this.Parse(token);
    }

    private Operation Parse(Token token)
    {
        return (token.Value, int.TryParse(token.Value, out var value)) switch
        {
            ("+", _) => new Operation(Opcode.PLUS, token.Location),
            (".", _) => new Operation(Opcode.DUMP, token.Location),
            (_, true)=> new Operation(Opcode.PUSH, token.Location, value),
            _ => throw new ArgumentException($"{token.Location} Unknown token {token.Value}"),
        };
    }

    private string FormatLocation(Location l) =>
        $"({l.Path}:{l.Row}:{l.Col})";
}

public class Logger
{
    public void LogError(string message)
    {
        this.Log("ERROR", message);
    }

    public void LogInfo(string message)
    {
        this.Log("INFO", message);
    }

    private void Log(string level, string message)
    {
        Console.WriteLine($"{level}: {message}");
    }
}

public class UsagePrinter
{
    private readonly ProgramContext context;

    public UsagePrinter(ProgramContext context)
    {
        this.context = context;
    }

    public void Print()
    {
        Console.WriteLine($"Usage: dotnet {this.context.ProgramName} [FILE]");
        Console.WriteLine($"  OPTIONS:");
        Console.WriteLine($"    -r|--run    Run the program after successful compilation");
    }
}

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

public class AssemblyGenerator
{
    public string Generate(IEnumerable<Operation> operations)
    {
        var sb = new StringBuilder();
        sb.AppendLine("segment .text");
        sb.AppendLine("global _start");
        sb.AppendLine("_start:");

        foreach (var op in operations)
            sb = this.Generate(op, sb);

        sb.AppendLine("    mov rax, 60");
        sb.AppendLine("    xor rdi, rdi");
        sb.AppendLine("    syscall");
        return sb.ToString();
    }

    private StringBuilder Generate(Operation op, StringBuilder sb)
    {
        sb.AppendLine($"    ; *** {op.Code} ***");
        sb = (op.Code, op.Value) switch
        {
            (Opcode.PUSH, int val) => this.GeneratePush(sb, val),
            (Opcode.PUSH, _) => throw new ArgumentException($"{op.location} Can only push integers"),
            (Opcode.PLUS, _) => this.GeneratePlus(sb),
            (Opcode.DUMP, _) => this.GenerateDump(sb),
            _ => throw new ArgumentException($"Operation ${op.Code} not supported")
        };
        return sb.AppendLine();
    }

    private StringBuilder GenerateDump(StringBuilder sb)
    {
        return sb;
    }

    private StringBuilder GeneratePlus(StringBuilder sb)
    {
        sb.AppendLine($"    pop rax");
        sb.AppendLine($"    pop rbx");
        sb.AppendLine($"    add rbx, rax");
        sb.AppendLine($"    push rbx");
        return sb;
    }

    private StringBuilder GeneratePush(StringBuilder sb, int value) =>
        sb.AppendLine($"    push {value}");
}

public class CommandRunner
{
    private readonly ProgramContext context;

    private readonly Logger logger;

    public CommandRunner(ProgramContext context, Logger logger)
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

class Program
{
    static void Main()
    {
        string[] args = Environment.GetCommandLineArgs();
        var context = new ProgramContext(args, Environment.Exit);
        var usagePrinter = new UsagePrinter(context);
        var logger = new Logger();
        var lexer = new Lexer(context, usagePrinter, logger);
        var parser = new Parser();
        var assemblyGenerator = new AssemblyGenerator();
        var commandRunner = new CommandRunner(context, logger);

        string path = string.Empty;
        var options = new HashSet<string>(new[] { "-r", "--run" });
        bool run = false;
        while (args.Length >= 2)
        {
            string arg = args[1];
            if      (args.Length == 2 && !options.Contains(arg)) path = arg;
            else if (arg == "-r" || arg == "--run") run = true;
            args = args.Take(1).Concat(args.Skip(2)).ToArray();
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            logger.LogError("No input file provided");
            usagePrinter.Print();
            context.Exit(1);
        }

        try
        {
            var tokens = lexer.LexFile(path);
            var operations = parser.Parse(tokens);
            var assemblyPath = Path.Combine(Environment.CurrentDirectory, Path.ChangeExtension(path, ".asm"));
            logger.LogInfo($"Generating assembly file {assemblyPath}");
            var assembly = assemblyGenerator.Generate(operations);
            File.WriteAllText(assemblyPath, assembly);
            var success = commandRunner.Run("nasm", "-felf64", assemblyPath);
            var binaryPath = Path.ChangeExtension(assemblyPath, null);
            if (success) success = commandRunner.Run(
                "ld",
                "-o",
                binaryPath,
                Path.ChangeExtension(assemblyPath, ".o"));
            if (run && success) success = commandRunner.Run(binaryPath);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            context.Exit(1);
        }
    }
}

public static class Extensions
{
    public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T> items)
    {
        int i = 0;
        foreach (var item in items)
            yield return (i++, item);
    }
}