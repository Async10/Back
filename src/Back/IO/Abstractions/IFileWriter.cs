namespace Back.IO.Abstractions;

public interface IFileWriter
{
    void WriteAllText(string filePath, string text);
}