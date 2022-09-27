namespace Back;

public abstract record Token(Location Location);

public record IntToken(int Value, Location Location)
    : Token(Location);

public record WordToken(string Value, Location Location)
    : Token(Location);