namespace Back;

public class Parser
{
    public IEnumerable<Operation> Parse(IEnumerable<Token> tokens)
    {
        foreach (var token in tokens)
            yield return this.Parse(token);
    }

    private Operation Parse(Token token)
    {
        return (token.Value, int.TryParse(token.Value, out var value)) switch
        {
            ("+", _) => new Operation(Opcode.PLUS, token.Location),
            (".", _) => new Operation(Opcode.DUMP, token.Location),
            (_, true)=> new Operation(Opcode.PUSH, token.Location, value),
            _ => throw new ArgumentException($"{token.Location} Unknown token {token.Value}"),
        };
    }

    private string FormatLocation(Location l) =>
        $"({l.Path}:{l.Row}:{l.Col})";
}
