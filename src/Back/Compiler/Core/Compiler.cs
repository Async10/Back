namespace Back.Compiler.Core;

using Back.AsssemblyGenerator.Abstractions;
using Back.Compiler.Abstractions;
using Back.Lexer.Abstractions;
using Back.Parser.Abstractions;
using Back.IO.Abstractions;
using Back.Shared.Abstractions;

public class Compiler : ICompiler
{
    private readonly ILexer lexer;

    private readonly IParser parser;

    private readonly IAssemblyGenerator assemblyGenerator;

    private readonly IFileWriter fileWriter;

    private readonly ICommandRunner commandRunner;

    private readonly ILogger logger;

    public Compiler(
        ILexer lexer,
        IParser parser,
        IAssemblyGenerator assemblyGenerator,
        IFileWriter fileWriter,
        ICommandRunner commandRunner,
        ILogger logger)
    {
        this.lexer = lexer;
        this.parser = parser;
        this.assemblyGenerator = assemblyGenerator;
        this.fileWriter = fileWriter;
        this.commandRunner = commandRunner;
        this.logger = logger;
    }

    public bool TryCompile(SourceFile file, bool run)
    {
        try
        {
            return Compile(file, run);
        }
        catch (Exception err)
        {
            this.logger.LogError(err.Message);
            return false;
        }
    }

    private bool Compile(SourceFile file, bool run)
    {
        var tokens = lexer.LexFile(file);
        var operations = parser.Parse(tokens);
        var assembly = assemblyGenerator.Generate(operations);
        var assemblyPath = Path.Combine(Environment.CurrentDirectory, Path.ChangeExtension(file.Path, ".asm"));

        this.logger.LogInfo($"Generating assembly file {assemblyPath}");
        this.fileWriter.WriteAllText(assemblyPath, assembly);

        var success = this.commandRunner.Run("nasm", "-felf64", assemblyPath);
        if (!success) return false;
        var binaryPath = Path.ChangeExtension(assemblyPath, null);
        success = this.commandRunner.Run(
            "ld",
            "-o",
            binaryPath,
            Path.ChangeExtension(assemblyPath, ".o"));
        if (!success) return false;
        if (run) success = this.commandRunner.Run(binaryPath);
        return success;
    }
}