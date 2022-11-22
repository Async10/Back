namespace Back.Lexer.Abstractions;

using Back.Shared.Abstractions;

public abstract record Token(Location Location);

public record LongToken(long Value, Location Location)
    : Token(Location);

public record WordToken(string Value, Location Location)
    : Token(Location);

public record StringToken(string Value, Location Location)
    : Token(Location);