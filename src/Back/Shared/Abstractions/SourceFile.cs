namespace Back.Shared.Abstractions;

public record SourceFile(string Path, IEnumerable<string> Lines);