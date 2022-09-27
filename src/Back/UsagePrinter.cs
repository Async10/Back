namespace Back;

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
