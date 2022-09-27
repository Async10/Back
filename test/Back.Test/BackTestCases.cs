namespace Back.Test;

using System.Diagnostics;
using Xunit;

public class BackTestCases
{
    private const string BackRelativePath = "src/Back/bin/Debug/net6.0/Back.dll";
    private const string FileRelativPath = "examples/test.back";

    [Fact]
    public void RunsTestBack()
    {
        var process = this.StartCompiler();
        process.WaitForExit();
        Assert.Equal(0, process.ExitCode);
        Assert.Equal("42", process.StandardOutput.ReadToEnd().Split('\n')[0]);
    }

    private Process StartCompiler()
    {
        string solutionPath = this.GetSolutionPath();
        string filePath = Path.Combine(solutionPath, FileRelativPath);
        string backPath = Path.Combine(solutionPath, BackRelativePath);
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"{backPath} -- --run --quiet {filePath}",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        });
        return process ?? throw new ArgumentNullException();
    }

    private string GetSolutionPath()
    {
        return Directory.GetParent(Environment.CurrentDirectory)
            ?.Parent
            ?.Parent
            ?.Parent
            ?.Parent
            ?.FullName ?? throw new ArgumentNullException();
    }
}