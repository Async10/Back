namespace Back.Parser.Core;

using Back.Lexer.Abstractions;
using Back.Parser.Abstractions;

public class Parser : IParser
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
            _ => throw new ArgumentException($"{token.Location} Undefined token"),
        };
    }

    private Operation Parse(IntToken token) =>
        new Operation(Opcode.Push, token.Location, token.Value);

    private Operation Parse(WordToken token) => token switch
    {
        { Value: "+" } => new Operation(Opcode.Plus, token.Location),
        { Value: "-" } => new Operation(Opcode.Sub, token.Location),
        { Value: "*" } => new Operation(Opcode.Mul, token.Location),
        { Value: "/" } => new Operation(Opcode.Div, token.Location),
        { Value: "%" } => new Operation(Opcode.Mod, token.Location),
        { Value: "divmod" } => new Operation(Opcode.DivMod, token.Location),
        { Value: "<" } => new Operation(Opcode.Less, token.Location),
        { Value: "<=" } => new Operation(Opcode.LessOrEqual, token.Location),
        { Value: "==" } => new Operation(Opcode.Equal, token.Location),
        { Value: ">" } => new Operation(Opcode.Greater, token.Location),
        { Value: ">=" } => new Operation(Opcode.GreaterOrEqual, token.Location),
        { Value: "drop" } => new Operation(Opcode.Drop, token.Location),
        { Value: "dup" } => new Operation(Opcode.Dup, token.Location),
        { Value: "over" } => new Operation(Opcode.Over, token.Location),
        { Value: "swap" } => new Operation(Opcode.Swap, token.Location),
        { Value: "rot" } => new Operation(Opcode.Rot, token.Location),
        { Value: "." } => new Operation(Opcode.Dump, token.Location),
        { Value: "emit" } => new Operation(Opcode.Emit, token.Location),
        _ => throw new ArgumentException($"{token.Location} Undefined token {token.Value}"),
    };
}
