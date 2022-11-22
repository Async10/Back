using System.Text.RegularExpressions;
namespace Back.Lexer.Core;

using Back.Lexer.Abstractions;
using Back.Shared.Abstractions;

public class Lexer : ILexer
{
    private const char EndOfLineCommentSymbol = '#';

    public IEnumerable<Token> LexFile(SourceFile file)
    {
        foreach ((int row, string line) in file.Lines.Enumerate())
            foreach ((int col, string value) in this.LexLine(line))
                yield return this.CreateToken(value, new Location(file.Path, row + 1, col + 1));
    }

    private IEnumerable<(int col, string word)> LexLine(string line)
    {
        int col = this.FindNonWhiteSpace(line);
        while (col != -1 && line[col] != EndOfLineCommentSymbol)
        {
            int end = this.FindEnd(line.Substring(col));
            if (end == -1)
            {
                yield return (col, line.Substring(col));
                col = end;
            }
            else
            {
                yield return (col, line.Substring(col, end));
                int colAddend = this.FindNonWhiteSpace(line.Substring(col + end));
                col = colAddend != -1 ? col + end + colAddend : colAddend;
            }
        }
    }

    private int FindEnd(string text)
    {
        if (text.StartsWith("'"))
            return this.FindIndex(
                text,
                (ch, idx, text) => (idx > 1 && this.IsWordEnd(ch) && text[idx - 1] == '\'' && text[idx - 2] != '\\'));

        if (text.StartsWith('"'))
            return this.FindIndex(
                text,
                (ch, idx, text) => (idx > 1 && this.IsWordEnd(ch) && text[idx - 1] == '"' && text[idx - 2] != '\\'));

        return this.FindIndex(text, this.IsWordEnd);
    }

    private bool IsWordEnd(char ch) =>
        char.IsWhiteSpace(ch) || ch == EndOfLineCommentSymbol;

    private int FindNonWhiteSpace(string text) =>
        this.FindIndex(text, ch => !Char.IsWhiteSpace(ch));

    private int FindIndex(string text, Func<char, int, string, bool> predicate)
    {
        for (int idx = 0; idx < text.Length; idx++)
            if (predicate(text[idx], idx, text))
                return idx;

        return -1;
    }

    private int FindIndex(string text, Func<char, bool> predicate) =>
        this.FindIndex(text, (ch, _, _) => predicate(ch));

    private Token CreateToken(string word, Location location)
    {
        if (word.StartsWith("'"))
        {
            if (char.TryParse(this.Unescape(word), out var aChar))
                return new LongToken(aChar, location);

            throw new ArgumentException($"{location} {word} is not a char");
        }

        if (word.StartsWith('"'))
        {
            if (word.EndsWith('"'))
                return new StringToken(this.Unescape(word), location);

            throw new ArgumentException($"{location} Unclosed string literal");
        }

        if (long.TryParse(word, out var i))
            return new LongToken(i, location);

        return new WordToken(word, location);
    }

    private string Unescape(string word) =>
        Regex.Unescape(word.Substring(1, word.Length - 2));
}
