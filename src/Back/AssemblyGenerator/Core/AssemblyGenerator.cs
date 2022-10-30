namespace Back.AsssemblyGenerator.Core;

using System.Text;
using Back.AsssemblyGenerator.Abstractions;
using Back.Parser.Abstractions;
using Back.Shared.Abstractions;

public partial class AssemblyGenerator : IAssemblyGenerator
{
    private const int MemoryCapacity = 640_000;

    private const int RetStackCapacity = 4_096;

    private const byte True = 1;

    private readonly IList<string> stringLiterals = new List<string>();

    public string Generate(IEnumerable<Operation> operations)
    {
        var sb = new StringBuilder();
        sb.AppendLine("segment .text");

        this.GenerateDumpFunction(sb);
        this.GenerateEmitFunction(sb);

        sb.AppendLine("global _start");
        sb.AppendLine("_start:");
        sb.AppendLine("mov rax, ret_stack_end");
        sb.AppendLine("mov [ret_stack_rsp], rax");

        foreach (var op in operations)
            this.Generate(op, sb);

        // Exit program
        sb.AppendLine("    mov rax, 60");
        sb.AppendLine("    xor rdi, rdi");
        sb.AppendLine("    syscall");

        // Allocate string literals
        sb.AppendLine( "segment .data");
        foreach (var (idx, stringLiteral) in this.stringLiterals.Enumerate())
            sb.AppendLine($"str_{idx}: db {string.Join(',', this.ConvertToHexValues(stringLiteral))}");

        sb.AppendLine( "segment .bss");
        sb.AppendLine($"ret_stack_rsp: resq 1");
        sb.AppendLine($"ret_stack: resb {RetStackCapacity}");
        sb.AppendLine($"ret_stack_end:");
        sb.AppendLine($"mem: resb {MemoryCapacity}");

        return sb.ToString();
    }

    private StringBuilder GenerateEmitFunction(StringBuilder sb)
    {
        sb.AppendLine("emit:");
        sb.AppendLine("    sub  rsp, 24");
        sb.AppendLine("    mov  edx, 1");
        sb.AppendLine("    mov  BYTE [rsp+12], dil");
        sb.AppendLine("    lea  rsi, [rsp+12]");
        sb.AppendLine("    mov  edi, 1");
        sb.AppendLine("    mov  rdx, 1");
        sb.AppendLine("    mov  rax, 1");
        sb.AppendLine("    syscall");
        sb.AppendLine("    add  rsp, 24");
        sb.AppendLine("    ret");
        return sb;
    }

    private StringBuilder GenerateDumpFunction(StringBuilder sb)
    {
        sb.AppendLine("dump:");
        sb.AppendLine("    mov r8, -3689348814741910323");
        sb.AppendLine("    sub rsp, 40");
        sb.AppendLine("    mov BYTE [rsp+20], 10");
        sb.AppendLine("    lea rcx, [rsp+19]");
        sb.AppendLine(".L2:");
        sb.AppendLine("    mov rax, rdi");
        sb.AppendLine("    mul r8");
        sb.AppendLine("    mov rax, rdi");
        sb.AppendLine("    shr rdx, 3");
        sb.AppendLine("    lea rsi, [rdx+rdx*4]");
        sb.AppendLine("    add rsi, rsi");
        sb.AppendLine("    sub rax, rsi");
        sb.AppendLine("    add eax, 48");
        sb.AppendLine("    mov BYTE [rcx], al");
        sb.AppendLine("    mov rax, rdi");
        sb.AppendLine("    mov rdi, rdx");
        sb.AppendLine("    mov rdx, rcx");
        sb.AppendLine("    sub rcx, 1");
        sb.AppendLine("    cmp rax, 9");
        sb.AppendLine("    ja  .L2");
        sb.AppendLine("    lea rax, [rsp+22]");
        sb.AppendLine("    mov edi, 1");
        sb.AppendLine("    mov rcx, rax");
        sb.AppendLine("    sub rcx, rdx");
        sb.AppendLine("    sub rdx, rax");
        sb.AppendLine("    lea rsi, [rsp+22+rdx]");
        sb.AppendLine("    mov rdx, rcx");
        sb.AppendLine("    sub rdx, 1");
        sb.AppendLine("    mov rax, 1");
        sb.AppendLine("    syscall");
        sb.AppendLine("    add rsp, 40");
        sb.AppendLine("    ret");
        return sb;
    }

    private StringBuilder Generate(Operation op, StringBuilder sb)
    {
        sb.AppendLine($"    ; --- {op.Code} ---");
        return op switch
        {
            IntOperation intOp => intOp.Code switch
            {
                Opcode.Push => this.GeneratePushInt(sb, intOp.Value),
                _           => throw new ArgumentException($"{intOp.Location} IntOperation {intOp.Code} not supported")
            },
            StringOperation stringOp => stringOp.Code switch
            {
                Opcode.Push => this.GeneratePushString(sb, stringOp.Value),
                _           => throw new ArgumentException($"{stringOp.Location} StingOperation {stringOp.Code} not suppoerted")
            },
            EndOperation endOp     => this.GenerateEnd(endOp, sb),
            ElseOperation elseOp   => this.GenerateElse(elseOp, sb),
            IfOperation ifOp       => this.GenerateIf(ifOp, sb),
            BeginOperation beginOp => this.GenerateBegin(beginOp, sb),
            WhileOperation whileOp => this.GenerateWhile(whileOp, sb),
            ProcOperation procOp   => this.GenerateProc(procOp, sb),
            CallOperation callOp   => this.GenerateCall(callOp, sb),
            ReturnOperation retOp  => this.GenerateReturn(retOp, sb),
            _ => op.Code switch
            {
                Opcode.Plus           => this.GeneratePlus(sb),
                Opcode.Sub            => this.GenerateSub(sb),
                Opcode.Mul            => this.GenerateMul(sb),
                Opcode.Div            => this.GenerateDiv(sb),
                Opcode.DivMod         => this.GenerateDivMod(sb),
                Opcode.Mod            => this.GenerateMod(sb),
                Opcode.Less           => this.GenerateLess(sb),
                Opcode.LessOrEqual    => this.GenerateLessOrEqual(sb),
                Opcode.Equal          => this.GenerateEqual(sb),
                Opcode.NotEqual       => this.GenerateNotEqual(sb),
                Opcode.Greater        => this.GenerateGreater(sb),
                Opcode.GreaterOrEqual => this.GenerateGreaterOrEqual(sb),
                Opcode.Drop           => this.GenerateDrop(sb),
                Opcode.Dup            => this.GenerateDup(sb),
                Opcode.Over           => this.GenerateOver(sb),
                Opcode.Swap           => this.GenerateSwap(sb),
                Opcode.Rot            => this.GenerateRot(sb),
                Opcode.Dump           => this.GenerateDump(sb),
                Opcode.Emit           => this.GenerateEmit(sb),
                Opcode.Mem            => this.GenerateMem(sb),
                Opcode.Store          => this.GenerateStore(sb),
                Opcode.Fetch          => this.GenerateFetch(sb),
                Opcode.Syscall3       => this.GenerateSyscall3(sb),
                _                     => throw new ArgumentException($"{op.Location} Can't generate assembly for unknown operation {op.Code}")
            }
        };
    }

    private StringBuilder GenerateReturn(ReturnOperation op, StringBuilder sb)
    {
        sb.AppendLine( "    mov rax, rsp");
        sb.AppendLine( "    mov rsp, [ret_stack_rsp]");
        sb.AppendLine( "    ret");
        sb.AppendLine($"addr_{op.ReturnAddress}:");
        return sb;
    }

    private StringBuilder GenerateCall(CallOperation op, StringBuilder sb)
    {
        sb.AppendLine($"    mov rax, rsp");
        sb.AppendLine($"    mov rsp, [ret_stack_rsp]");
        sb.AppendLine($"    call addr_{op.ProcAddress}");
        sb.AppendLine( "    mov [ret_stack_rsp], rsp");
        sb.AppendLine( "    mov rsp, rax");
        // sb.AppendLine($"addr_{op.CallAddress}:");
        return sb;
    }

    private StringBuilder GenerateProc(ProcOperation op, StringBuilder sb)
    {
        sb.AppendLine($"    jmp addr_{op.ReturnAddress}");
        sb.AppendLine($"addr_{op.ProcAddress}:");
        sb.AppendLine($"    mov [ret_stack_rsp], rsp");
        sb.AppendLine($"    mov rsp, rax");
        return sb;
    }

    private StringBuilder GeneratePushString(StringBuilder sb, string value)
    {
        sb.AppendLine($"    push str_{this.stringLiterals.Count}");
        sb.AppendLine($"    mov rax, {value.Length}");
        sb.AppendLine($"    push rax");
        this.stringLiterals.Add(value);
        return sb;
    }

    private StringBuilder GenerateSyscall3(StringBuilder sb)
    {
        sb.AppendLine("    pop rdx");
        sb.AppendLine("    pop rsi");
        sb.AppendLine("    pop rdi");
        sb.AppendLine("    pop rax");
        sb.AppendLine("    syscall");
        return sb;
    }

    private StringBuilder GenerateFetch(StringBuilder sb)
    {
        sb.AppendLine("    pop rax");
        sb.AppendLine("    xor rbx, rbx");
        sb.AppendLine("    mov bl, BYTE [rax]");
        sb.AppendLine("    push rbx");
        return sb;
    }

    private StringBuilder GenerateStore(StringBuilder sb)
    {
        sb.AppendLine("    pop rbx");
        sb.AppendLine("    pop rax");
        sb.AppendLine("    mov [rax], bl");
        return sb;
    }

    private StringBuilder GenerateMem(StringBuilder sb)
    {
        sb.AppendLine("    push mem");
        return sb;
    }

    private StringBuilder GenerateWhile(WhileOperation op, StringBuilder sb)
    {
        sb.AppendLine($"    pop rax");
        sb.AppendLine($"    test rax, rax");
        sb.AppendLine($"    jz addr_{op.EndAddress}");
        return sb;
    }

    private StringBuilder GenerateBegin(BeginOperation op, StringBuilder sb)
    {
        sb.AppendLine($"addr_{op.BeginAddress}:");
        return sb;
    }

    private StringBuilder GenerateElse(ElseOperation op, StringBuilder sb)
    {
        sb.AppendLine($"    jmp addr_{op.EndAddress}");
        sb.AppendLine($"addr_{op.ElseAddress}:");
        return sb;
    }

    private StringBuilder GenerateIf(IfOperation op, StringBuilder sb)
    {
        sb.AppendLine($"    pop rax");
        sb.AppendLine($"    test rax, rax");
        sb.AppendLine($"    jz addr_{op.ElseOrEndAddress}");
        return sb;
    }

    private StringBuilder GenerateEnd(EndOperation op, StringBuilder sb)
    {
        if (op.BeginAddress != -1)
        {
            sb.AppendLine($"    jmp addr_{op.BeginAddress}");
        }

        return sb.AppendLine($"addr_{op.EndAddress}:");
    }

    private StringBuilder GenerateEmit(StringBuilder sb)
    {
        sb.AppendLine("    pop rdi");  // emit expects argument in rdi register
        sb.AppendLine("    call emit");
        return sb;
    }

    private StringBuilder GenerateGreaterOrEqual(StringBuilder sb) =>
        this.GenerateComparison(sb, "cmovge");
    private StringBuilder GenerateGreater(StringBuilder sb) =>
        this.GenerateComparison(sb, "cmovg");

    private StringBuilder GenerateEqual(StringBuilder sb) =>
        this.GenerateComparison(sb, "cmove");

    private StringBuilder GenerateNotEqual(StringBuilder sb) =>
        this.GenerateComparison(sb, "cmovne");

    private StringBuilder GenerateLessOrEqual(StringBuilder sb) =>
        this.GenerateComparison(sb, "cmovle");

    private StringBuilder GenerateLess(StringBuilder sb) =>
        this.GenerateComparison(sb, "cmovs");

    private StringBuilder GenerateComparison(StringBuilder sb, string cmovInstruction)
    {
        sb.AppendLine( "    pop rbx");
        sb.AppendLine( "    pop rax");
        sb.AppendLine($"    mov rcx, {True}");
        sb.AppendLine( "    xor rdx, rdx");
        sb.AppendLine( "    cmp rax, rbx");
        sb.AppendLine($"    {cmovInstruction} rdx, rcx");
        sb.AppendLine( "    push rdx");
        return sb;
    }

    private StringBuilder GenerateRot(StringBuilder sb)
    {
        sb.AppendLine("    pop rcx");
        sb.AppendLine("    pop rbx");
        sb.AppendLine("    pop rax");
        sb.AppendLine("    push rbx");
        sb.AppendLine("    push rcx");
        sb.AppendLine("    push rax");
        return sb;
    }

    private StringBuilder GenerateSwap(StringBuilder sb)
    {
        sb.AppendLine("    pop rax");
        sb.AppendLine("    pop rbx");
        sb.AppendLine("    push rax");
        sb.AppendLine("    push rbx");
        return sb;
    }

    private StringBuilder GenerateOver(StringBuilder sb)
    {
        sb.AppendLine("    pop rax");
        sb.AppendLine("    pop rbx");
        sb.AppendLine("    push rbx");
        sb.AppendLine("    push rax");
        sb.AppendLine("    push rbx");
        return sb;
    }

    private StringBuilder GenerateDup(StringBuilder sb)
    {
        sb.AppendLine("    pop rax");
        sb.AppendLine("    push rax");
        sb.AppendLine("    push rax");
        return sb;
    }

    private StringBuilder GenerateDrop(StringBuilder sb)
    {
        sb.AppendLine("    pop rax");
        return sb;
    }

    private StringBuilder GenerateMod(StringBuilder sb)
    {
        this.GenerateDivMod(sb);
        this.GenerateSwap(sb);
        this.GenerateDrop(sb);
        return sb;
    }

    private StringBuilder GenerateDivMod(StringBuilder sb)
    {
        sb.AppendLine("    xor rdx, rdx");
        sb.AppendLine("    pop rbx");
        sb.AppendLine("    pop rax");
        sb.AppendLine("    div rbx");
        sb.AppendLine("    push rax");
        sb.AppendLine("    push rdx");
        return sb;
    }

    private StringBuilder GenerateDiv(StringBuilder sb)
    {
        this.GenerateDivMod(sb);
        this.GenerateDrop(sb);
        return sb;
    }

    private StringBuilder GenerateMul(StringBuilder sb)
    {
        sb.AppendLine("    pop rbx");
        sb.AppendLine("    pop rax");
        sb.AppendLine("    mul rbx");
        sb.AppendLine("    push rax");
        return sb;
    }

    private StringBuilder GenerateSub(StringBuilder sb)
    {
        sb.AppendLine("    pop rax");
        sb.AppendLine("    pop rbx");
        sb.AppendLine("    sub rbx, rax");
        sb.AppendLine("    push rbx");
        return sb;
    }

    private StringBuilder GeneratePlus(StringBuilder sb)
    {
        sb.AppendLine("    pop rax");
        sb.AppendLine("    pop rbx");
        sb.AppendLine("    add rbx, rax");
        sb.AppendLine("    push rbx");
        return sb;
    }

    private StringBuilder GenerateDump(StringBuilder sb)
    {
        sb.AppendLine("    pop rdi");  // dump expects argument in rdi register
        sb.AppendLine("    call dump");
        return sb;
    }

    private StringBuilder GeneratePushInt(StringBuilder sb, int value) =>
        sb.AppendLine($"    push {value}");

    private IEnumerable<string> ConvertToHexValues(string text) =>
        Encoding.UTF8.GetBytes(text).Select(b => $"0x{b.ToString("X2")}");
}
