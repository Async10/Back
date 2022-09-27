namespace Back;

using System.Linq;

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

        var tokens = lexer.LexFile(path);
        var operations = parser.Parse(tokens);
        var assemblyPath = Path.Combine(Environment.CurrentDirectory, Path.ChangeExtension(path, ".asm"));
        logger.LogInfo($"Generating assembly file {assemblyPath}");
        var assembly = assemblyGenerator.Generate(operations);
        File.WriteAllText(assemblyPath, assembly);
        var success = commandRunner.Run("nasm", "-felf64", assemblyPath);
        if (!success)
        {
            context.Exit(1);
        }

        var binaryPath = Path.ChangeExtension(assemblyPath, null);
        success = commandRunner.Run(
            "ld",
            "-o",
            binaryPath,
            Path.ChangeExtension(assemblyPath, ".o"));
        if (!success)
        {
            context.Exit(1);
        }

        if (run)
        {
            commandRunner.Run(binaryPath);
        }
    }
}
