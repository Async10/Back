namespace Back;

using System.Linq;

class Program
{
    static void Main()
    {
        string[] args = Environment.GetCommandLineArgs();
        var commandLineArgs = CommandLineArgsParser.Parse(args);
        var context = new ExecutionContext(commandLineArgs, Environment.Exit);
        var usagePrinter = new UsagePrinter(context);
        var logger = new Logger()
        {
            Quiet = commandLineArgs.Quiet,
        };

        var lexer = new Lexer(context, usagePrinter, logger);
        var parser = new Parser();
        var assemblyGenerator = new AssemblyGenerator();
        var commandRunner = new CommandRunner(context, logger);

        if (string.IsNullOrWhiteSpace(commandLineArgs.FilePath))
        {
            logger.LogError("No input file provided");
            usagePrinter.Print();
            context.Exit(1);
        }

        var tokens = lexer.LexFile(commandLineArgs.FilePath);
        var operations = parser.Parse(tokens);
        var assemblyPath = Path.Combine(Environment.CurrentDirectory, Path.ChangeExtension(commandLineArgs.FilePath, ".asm"));
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

        if (commandLineArgs.Run)
        {
            commandRunner.Run(binaryPath);
        }
    }
}
