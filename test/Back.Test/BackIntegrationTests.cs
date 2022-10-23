namespace Back.Test;

using System.Diagnostics;
using System.Text.Json;
using Xunit;

public class BackIntegrationTests
{
    private const string BackRelativePath = "src/Back/bin/Debug/net6.0/Back.dll";

    private const string TestCasesDirectoryName = "examples";

    private const string ResultsDirectoryName = "IntegrationTestResults";

    [Theory]
    [InlineData("arithmetic.back")]
    [InlineData("stack-manip.back")]
    [InlineData("comparison.back")]
    [InlineData("hello-world.back")]
    [InlineData("if-else.back")]
    [InlineData("while.back")]
    [InlineData("fizz-buzz.back")]
    [InlineData("mem.back")]
    [InlineData("syscall3.back")]
    public void RunsIntegrationTests(string fileName)
    {
        var process = this.StartCompiler(fileName);
        process.WaitForExit();
        var result = this.GetExpectedResult(fileName);
        Assert.Equal(result.ExitCode, process.ExitCode);
        var actualOutput = process.StandardOutput.ReadToEnd();
        foreach (var (expectedLine, actualLine) in result.Output.Zip(actualOutput.Split('\n')))
            Assert.Equal(expectedLine, actualLine);
    }

    private IntegrationTestResult GetExpectedResult(string fileName)
    {
        string resultFileName = $"{Path.GetFileNameWithoutExtension(fileName)}.result.json";
        string json = File.ReadAllText(
            Path.Combine(Environment.CurrentDirectory, ResultsDirectoryName, resultFileName));
        return JsonSerializer.Deserialize<IntegrationTestResult>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        })
            ?? throw new ArgumentNullException();
    }

    private string SolutionPath =>
        Directory.GetParent(Environment.CurrentDirectory)
            ?.Parent
            ?.Parent
            ?.Parent
            ?.Parent
            ?.FullName ?? throw new ArgumentNullException();

    private Process StartCompiler(string fileName)
    {
        string filePath = Path.Combine(this.SolutionPath, "examples", fileName);
        string backPath = Path.Combine(this.SolutionPath, BackRelativePath);
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"{backPath} -- --run --quiet {filePath}",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        });
        return process ?? throw new ArgumentNullException();
    }
}