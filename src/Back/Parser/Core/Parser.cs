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
        new Operation(Opcode.PUSH, token.Location, token.Value);

    private Operation Parse(WordToken token) => token switch
    {
        { Value: "+" } => new Operation(Opcode.PLUS, token.Location),
        { Value: "-" } => new Operation(Opcode.SUB, token.Location),
        { Value: "*" } => new Operation(Opcode.MUL, token.Location),
        { Value: "/" } => new Operation(Opcode.DIV, token.Location),
        { Value: "%" } => new Operation(Opcode.MOD, token.Location),
        { Value: "divmod" } => new Operation(Opcode.DIVMOD, token.Location),
        { Value: "<" } => new Operation(Opcode.LESS, token.Location),
        { Value: "<=" } => new Operation(Opcode.LESS_OR_EQUAL, token.Location),
        { Value: "==" } => new Operation(Opcode.EQUAL, token.Location),
        { Value: ">" } => new Operation(Opcode.GREATER, token.Location),
        { Value: "drop" } => new Operation(Opcode.DROP, token.Location),
        { Value: "dup" } => new Operation(Opcode.DUP, token.Location),
        { Value: "over" } => new Operation(Opcode.OVER, token.Location),
        { Value: "swap" } => new Operation(Opcode.SWAP, token.Location),
        { Value: "rot" } => new Operation(Opcode.ROT, token.Location),
        { Value: "." } => new Operation(Opcode.DUMP, token.Location),
        _ => throw new ArgumentException($"{token.Location} Undefined token {token.Value}"),
    };
}
