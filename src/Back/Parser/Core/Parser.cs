using System.Text;
namespace Back.Parser.Core;

using System.Collections.Generic;
using Back.Lexer.Abstractions;
using Back.Parser.Abstractions;
using Back.Shared.Abstractions;

public class Parser : IParser
{
    public IEnumerable<Operation> Parse(IEnumerable<Token> tokens)
    {
        var operations = tokens.Enumerate().ToDictionary(
            keySelector: tuple => tuple.Item1,
            elementSelector: tuple => this.Parse(tuple.Item2, tuple.Item1));
        return this.CloseBlocks(operations);
    }

    private IEnumerable<Operation> CloseBlocks(IReadOnlyDictionary<int, Operation> operations)
    {
        Stack<int> ips = new();
        Dictionary<int, Operation> result = new();

        void HandleIf(IfOperation op, int ip)
        {
            ips.Push(ip);
        }

        void HandleElse(ElseOperation op, int ip)
        {
            int ifIp = ips.Pop();
            if (result[ifIp] is IfOperation ifOp)
            {
                result[ifIp] = ifOp with { ElseOrEndAddress = op.ElseAddress };
            }

            ips.Push(ip);
        }

        void HandleEnd(EndOperation op, int ip)
        {
            int blockOpIp = ips.Pop();
            if (result[blockOpIp] is ElseOperation elseOp)
            {
                result[blockOpIp] = elseOp with { EndAddress = op.EndAddress };
            }
            else if (result[blockOpIp] is IfOperation ifOp)
            {
                result[blockOpIp] = ifOp with { ElseOrEndAddress = op.EndAddress };
            }
            else if (result[blockOpIp] is WhileOperation whileOp)
            {
                result[blockOpIp] = whileOp with { EndAddress = op.EndAddress };
                if (result[ips.Pop()] is BeginOperation beginOp)
                {
                    result[ip] = op with { BeginAddress = beginOp.BeginAddress };
                }
            }
        }

        void HandleBegin(BeginOperation op, int ip)
        {
            ips.Push(ip);
        }

        void HandleWhile(WhileOperation op, int ip)
        {
            ips.Push(ip);
        }

        foreach (var (ip, op) in operations)
        {
            result[ip] = op;
            if (op is IfOperation ifOp)            HandleIf(ifOp, ip);
            else if (op is ElseOperation elseOp)   HandleElse(elseOp, ip);
            else if (op is EndOperation endOp)     HandleEnd(endOp, ip);
            else if (op is BeginOperation beginOp) HandleBegin(beginOp, ip);
            else if (op is WhileOperation whileOp) HandleWhile(whileOp, ip);
        }

        this.EnsureAllBlocksClosed(result, ips);

        return result.Values;
    }

    private void EnsureAllBlocksClosed(IReadOnlyDictionary<int, Operation> operations, Stack<int> ips)
    {
        if (ips.Count > 0)
        {
            var message = new StringBuilder();
            while (ips.Count > 0)
            {
                message.AppendLine($"{operations[ips.Pop()].Location} unclosed block");
            }

            throw new ArgumentException(message.ToString().Substring(0, message.Length - 1));
        }
    }

    private Operation Parse(Token token, int instructionPointer)
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
        { Value: "if" } => new IfOperation(token.Location),
        { Value: "end" } => new EndOperation(token.Location, EndAddress: instructionPointer),
        { Value: "else" } => new ElseOperation(token.Location, ElseAddress: instructionPointer),
        { Value: "begin" } => new BeginOperation(token.Location, BeginAddress: instructionPointer),
        { Value: "while" } => new WhileOperation(token.Location),
        _ => throw new ArgumentException($"{token.Location} Undefined token {token.Value}"),
    };
}
