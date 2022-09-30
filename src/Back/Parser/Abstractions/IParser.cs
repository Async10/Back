namespace Back.Parser.Abstractions;

using Back.Lexer.Abstractions;

public interface IParser
{
    IEnumerable<Operation> Parse(IEnumerable<Token> tokens);
}
