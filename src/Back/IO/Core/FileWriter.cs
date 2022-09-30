namespace Back.IO.Core;

using System.IO;
using Back.IO.Abstractions;

public class FileWriter : IFileWriter
{
    public void WriteAllText(string filePath, string text)
    {
        File.WriteAllText(filePath, text);
    }
}