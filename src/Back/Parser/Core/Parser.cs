namespace Back.Parser.Core;

using System.Collections.Generic;
using System.Text;
using Back.Lexer.Abstractions;
using Back.Parser.Abstractions;
using Back.Shared.Abstractions;

public class Parser : IParser
{
    public IEnumerable<Operation> Parse(IEnumerable<Token> tokens)
    {
        List<Operation> operations = new();
        Queue<Token> tokenQueue = new(tokens);
        while (tokenQueue.TryDequeue(out var token))
            operations.Add(this.ParseToken(token, tokenQueue));

        return this.CloseBlocks(operations);
    }

    private IEnumerable<Operation> CloseBlocks(IEnumerable<Operation> operations)
    {
        Stack<int> ips = new();
        Dictionary<string, int> procIps = new();
        Dictionary<int, Operation> result = new();
        int? currentProcIp = null;

        Operation HandleIf(IfOperation op, int ip)
        {
            ips.Push(ip);
            return op;
        }

        Operation HandleElse(ElseOperation op, int ip)
        {
            op = op with { ElseAddress = ip };

            int ifIp = ips.Pop();
            if (result[ifIp] is IfOperation ifOp)
            {
                result[ifIp] = ifOp with { ElseOrEndAddress = op.ElseAddress };
            }

            ips.Push(op.ElseAddress);

            return op;
        }

        Operation HandleEnd(EndOperation op, int ip)
        {
            op = op with { EndAddress = ip };

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
                    op = op with { BeginAddress = beginOp.BeginAddress };
            }
            else if (result[blockOpIp] is ProcOperation procOp)
            {
                result[blockOpIp] = procOp with { ReturnAddress = ip };
                currentProcIp = null;
                return new ReturnOperation(op.Location, ip);
            }

            return op;
        }

        Operation HandleBegin(BeginOperation op, int ip)
        {
            op = op with { BeginAddress = ip };
            ips.Push(op.BeginAddress);
            return op;
        }

        Operation HandleWhile(WhileOperation op, int ip)
        {
            ips.Push(ip);
            return op;
        }

        Operation HandleProc(ProcOperation op, int ip)
        {
            if (currentProcIp != null)
                throw new ArgumentException(
                    $"{op.Location} Defining procedures inside of procedures is not allowed");

            op = op with { ProcAddress = ip };
            if (!procIps.TryAdd(op.ProcName, op.ProcAddress))
                throw new ArgumentException(
                    $"{op.Location} Procedure name {op.ProcName} already exists");

            currentProcIp = op.ProcAddress;
            ips.Push(op.ProcAddress);
            return op;
        }

        Operation HandleCall(CallOperation callOp, int ip)
        {
            if (procIps.TryGetValue(callOp.ProcName, out int procAddress))
                return callOp with { ProcAddress = procAddress, CallAddress = ip };

            throw new ArgumentException(
                $"{callOp.Location} Procedure with {callOp.ProcName} doesn't exist");
        }

        foreach (var (ip, op) in operations.Enumerate())
        {
            result[ip] = op switch
            {
                IfOperation ifOp       => HandleIf(ifOp, ip),
                ElseOperation elseOp   => HandleElse(elseOp, ip),
                EndOperation endOp     => HandleEnd(endOp, ip),
                BeginOperation beginOp => HandleBegin(beginOp, ip),
                WhileOperation whileOp => HandleWhile(whileOp, ip),
                ProcOperation procOp   => HandleProc(procOp, ip),
                CallOperation callOp   => HandleCall(callOp, ip),
                _                      => op,
            };
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
                Operation op = operations[ips.Pop()];
                message.AppendLine($"{op.Location} unclosed {op.Code.ToString().ToLower()}-block");
            }

            throw new ArgumentException(message.ToString().Substring(0, message.Length - 1));
        }
    }

    private Operation ParseToken(Token token, Queue<Token> tokenQueue)
    {
        return token switch
        {
            LongToken intToken => this.ParseLongToken(intToken),
            StringToken stringToken => this.ParseStringToken(stringToken),
            WordToken wordToken => this.ParseWordToken(wordToken, tokenQueue),
            _ => throw new ArgumentException($"{token.Location} Undefined token"),
        };
    }

    private Operation ParseStringToken(StringToken token) =>
        new StringOperation(Opcode.Push, token.Location, token.Value);

    private Operation ParseLongToken(LongToken token) =>
        new LongOperation(Opcode.Push, token.Location, token.Value);

    private Operation ParseWordToken(WordToken token, Queue<Token> tokenQueue) => token switch
    {
        { Value: "+" }        => new Operation(Opcode.Plus, token.Location),
        { Value: "-" }        => new Operation(Opcode.Sub, token.Location),
        { Value: "*" }        => new Operation(Opcode.Mul, token.Location),
        { Value: "/" }        => new Operation(Opcode.Div, token.Location),
        { Value: "%" }        => new Operation(Opcode.Mod, token.Location),
        { Value: "divmod" }   => new Operation(Opcode.DivMod, token.Location),
        { Value: "<" }        => new Operation(Opcode.Less, token.Location),
        { Value: "<=" }       => new Operation(Opcode.LessOrEqual, token.Location),
        { Value: "==" }       => new Operation(Opcode.Equal, token.Location),
        { Value: "<>" }       => new Operation(Opcode.NotEqual, token.Location),
        { Value: ">" }        => new Operation(Opcode.Greater, token.Location),
        { Value: ">=" }       => new Operation(Opcode.GreaterOrEqual, token.Location),
        { Value: "rshift" }   => new Operation(Opcode.RShift, token.Location),
        { Value: "lshift" }   => new Operation(Opcode.LShift, token.Location),
        { Value: "and" }      => new Operation(Opcode.BitwiseAnd, token.Location),
        { Value: "or" }       => new Operation(Opcode.BitwiseOr, token.Location),
        { Value: "drop" }     => new Operation(Opcode.Drop, token.Location),
        { Value: "dup" }      => new Operation(Opcode.Dup, token.Location),
        { Value: "over" }     => new Operation(Opcode.Over, token.Location),
        { Value: "swap" }     => new Operation(Opcode.Swap, token.Location),
        { Value: "rot" }      => new Operation(Opcode.Rot, token.Location),
        { Value: "." }        => new Operation(Opcode.Dump, token.Location),
        { Value: "emit" }     => new Operation(Opcode.Emit, token.Location),
        { Value: "if" }       => new IfOperation(token.Location),
        { Value: "end" }      => new EndOperation(token.Location),
        { Value: "else" }     => new ElseOperation(token.Location),
        { Value: "begin" }    => new BeginOperation(token.Location),
        { Value: "while" }    => new WhileOperation(token.Location),
        { Value: "mem" }      => new Operation(Opcode.Mem, token.Location),
        { Value: "!" }        => new Operation(Opcode.Store, token.Location),
        { Value: "@" }        => new Operation(Opcode.Fetch, token.Location),
        { Value: "syscall3" } => new Operation(Opcode.Syscall3, token.Location),
        { Value: "proc" }     => this.ParseProc(token.Location, tokenQueue),
        _                     => new CallOperation(token.Location, token.Value),
        // _                     => throw new ArgumentException($"{token.Location} Undefined token '{token.Value}'"),
    };

    private Operation ParseWord(string word, Location location)
    {
        throw new NotImplementedException();
    }

    private ProcOperation ParseProc(Location location, Queue<Token> tokenQueue)
    {
        if (tokenQueue.TryDequeue(out var token))
        {
            if (token is WordToken name)
            {
                return new ProcOperation(location, name.Value);
            }

            throw new ArgumentException($"{token.Location} Procedure name has to be a word");
        }

        throw new ArgumentException($"{location} Expected procedure name but found nothing");
    }
}
