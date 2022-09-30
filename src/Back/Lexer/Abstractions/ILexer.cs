namespace Back.Lexer.Abstractions;

using Back.Shared.Abstractions;

public interface ILexer
{
    IEnumerable<Token> LexFile(SourceFile file);
}
