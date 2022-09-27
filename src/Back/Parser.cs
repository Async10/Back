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
        return token switch
        {
            IntToken intToken => this.Parse(intToken),
            WordToken wordToken => this.Parse(wordToken),
            _ => throw new ArgumentException($"{token.Location} Unknown token"),
        };
    }

    private Operation Parse(IntToken token) =>
        new Operation(Opcode.PUSH, token.Location, token.Value);

    private Operation Parse(WordToken token) => token switch
    {
        { Value: "+" } => new Operation(Opcode.PLUS, token.Location),
        { Value: "." } => new Operation(Opcode.DUMP, token.Location),
        _ => throw new ArgumentException($"{token.Location} Unknown token {token.Value}"),
    };

    private string FormatLocation(Location l) =>
        $"({l.Path}:{l.Row}:{l.Col})";
}
