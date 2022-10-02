namespace Back.Test;

using System.Diagnostics;
using System.Text.Json;
using Xunit;

public class BackTestCases
{
    private const string BackRelativePath = "src/Back/bin/Debug/net6.0/Back.dll";
    private const string TestCasesDirectoryName = "examples";

    [Theory]
    [InlineData("arithmetic.back")]
    [InlineData("stack-manip.back")]
    [InlineData("comparison.back")]
    [InlineData("hello-world.back")]
    public void RunsTestBack(string fileName)
    {
        var process = this.StartCompiler(fileName);
        process.WaitForExit();
        var result = this.GetTestCaseResult(fileName);
        Assert.Equal(result.ExitCode, process.ExitCode);
        var actualOutput = process.StandardOutput.ReadToEnd();
        foreach (var (expectedLine, actualLine) in result.Output.Zip(actualOutput.Split('\n')))
            Assert.Equal(expectedLine, actualLine);
    }

    private TestCaseResult GetTestCaseResult(string fileName)
    {
        string resultFileName = $"{Path.GetFileNameWithoutExtension(fileName)}.result.json";
        string json = File.ReadAllText(
            Path.Combine(this.SolutionPath, TestCasesDirectoryName, resultFileName));
        return JsonSerializer.Deserialize<TestCaseResult>(json, new JsonSerializerOptions
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