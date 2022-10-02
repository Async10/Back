using System.Text;
using System.Reflection.Emit;
namespace Back.Parser.Core;

using Back.Lexer.Abstractions;
using Back.Parser.Abstractions;
using Back.Shared.Abstractions;

public class Parser : IParser
{
    public IEnumerable<Operation> Parse(IEnumerable<Token> tokens)
    {
        Stack<(int, Operation)> instructionPointers = new();
        Dictionary<int, Operation> operations = new();
        foreach (var (instructionPointer, token) in tokens.Enumerate())
        {
            var operation = this.Parse(instructionPointer, token);
            if (operation.Code == Opcode.If)
            {
                instructionPointers.Push((instructionPointer, operation));
            }
            else if (operation is BlockOperation { Code: Opcode.End } blockOp)
            {
                int ifInstructionPointer = instructionPointers.Pop().Item1;
                var ifOperation = operations[ifInstructionPointer];
                operations[ifInstructionPointer] = new BlockOperation(
                    ifOperation.Code, ifOperation.Location, blockOp.Label);
            }

            operations[instructionPointer] = operation;
        }

        if (instructionPointers.Count > 0 )
        {
            var message = new StringBuilder();
            while (instructionPointers.Count > 0)
            {
                var (_, operation) = instructionPointers.Pop();
                message.AppendLine($"{operation.Location} unclosed if block");
            }

            throw new ArgumentException(message.ToString().Substring(0, message.Length - 1));
        }

        return operations.Values;
    }

    private Operation Parse(int instructionPointer, Token token)
    {
        return token switch
        {
            IntToken intToken => this.Parse(intToken),
            WordToken wordToken => this.Parse(wordToken, instructionPointer),
            _ => throw new ArgumentException($"{token.Location} Undefined token"),
        };
    }

    private Operation Parse(IntToken token) =>
        new IntOperation(Opcode.Push, token.Location, token.Value);

    private Operation Parse(WordToken token, int instructionPointer) => token switch
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
        { Value: "if" } => new Operation(Opcode.If, token.Location),
        { Value: "end" } => new BlockOperation(Opcode.End, token.Location, instructionPointer),
        _ => throw new ArgumentException($"{token.Location} Undefined token {token.Value}"),
    };
}
