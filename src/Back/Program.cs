namespace Back;

using System;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Back.Shared.Abstractions;
using Back.Compiler.Abstractions;
using Back.Compiler.DI;
using Back.IO.DI;
using Back.Lexer.DI;
using Back.Parser.DI;
using Back.AsssemblyGenerator.DI;
using Back.IO.Abstractions;

class Program
{
    private static readonly CommandLineArgs Args;

    private static readonly ILogger Logger;

    private static readonly ICompiler Compiler;

    static Program()
    {
        Args = CommandLineArgs.Parse(Environment.GetCommandLineArgs());
        var services = ConfigureServices(Args);
        Logger = services.GetRequiredService<ILogger>();
        Compiler = services.GetRequiredService<ICompiler>();
    }

    static int Main()
    {
        if (string.IsNullOrWhiteSpace(Args.FilePath))
        {
            Logger.LogError("No input file provided");
            PrintUsage();
            return 1;
        }

        if (!File.Exists(Args.FilePath))
        {
            Logger.LogError($"File {Args.FilePath} does not exist");
            return 1;
        }

        var lines = File.ReadAllLines(Args.FilePath, Encoding.UTF8);
        if (Compiler.TryCompile(
                new SourceFile(Args.FilePath, lines),
                Args.Run))
        {
            return 0;
        }

        return 1;
    }

    private static IServiceProvider ConfigureServices(CommandLineArgs args)
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
                services
                    .AddIO(logger =>
                    {
                        logger.Quiet = true;
                    })
                    .AddLexer()
                    .AddParser()
                    .AddAssemblyGenerator()
                    .AddCompiler())
            .Build()
            .Services;
    }

    private static void PrintUsage()
    {
        Console.WriteLine($"Usage: dotnet {Args.ProgramName} [FILE]");
        Console.WriteLine($"  OPTIONS:");
        Console.WriteLine($"    -r|--run        Run the program after successful compilation");
        Console.WriteLine($"    -q|--quiet      Be quiet, only report errors.");
    }
}
