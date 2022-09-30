namespace Back.Shared.Abstractions;

public record Location(string Path, int Row, int Col)
{
    public override string ToString() =>
        $"({this.Path}:{this.Row}:{this.Col})";
}
