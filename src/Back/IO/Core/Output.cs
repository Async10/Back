namespace Back.IO.Core;

using System;
using Back.IO.Abstractions;

public class Output : IOutput
{
    public void WriteLine(string line)
    {
        Console.WriteLine(line);
    }
}