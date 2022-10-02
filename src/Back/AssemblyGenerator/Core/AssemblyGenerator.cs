using System.Security.Cryptography.X509Certificates;
using System.Reflection.Emit;
namespace Back.AsssemblyGenerator.Core;

using System.Text;
using Back.AsssemblyGenerator.Abstractions;
using Back.Parser.Abstractions;

public partial class AssemblyGenerator : IAssemblyGenerator
{
    private const byte True = 1;

    public string Generate(IEnumerable<Operation> operations)
    {
        var sb = new StringBuilder();
        sb.AppendLine("segment .text");

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

        sb.AppendLine("global _start");
        sb.AppendLine("_start:");

        foreach (var op in operations)
        {
            sb.AppendLine($"    ; *** {op.Code} ***");
            sb = this.Generate(op, sb);
        }

        sb.AppendLine("    mov rax, 60");
        sb.AppendLine("    xor rdi, rdi");
        sb.AppendLine("    syscall");
        return sb.ToString();
    }

    private StringBuilder Generate(Operation op, StringBuilder sb) =>
        op switch
        {
            IntOperation intOp => intOp.Code switch
            {
                Opcode.Push => this.GeneratePush(sb, intOp.Value),
                _ => throw new ArgumentException($"{intOp.Location} IntOperation {intOp.Code} not supported")
            },
            BlockOperation blockOp => blockOp.Code switch
            {
                Opcode.If => this.GenerateIf(blockOp, sb),
                Opcode.End => this.GenerateEnd(blockOp, sb),
                _ => throw new ArgumentException($"{blockOp.Location} BlockOperation {blockOp.Code} not supported")
            },
            _ => op.Code switch
            {
                Opcode.Plus => this.GeneratePlus(sb),
                Opcode.Sub => this.GenerateSub(sb),
                Opcode.Mul => this.GenerateMul(sb),
                Opcode.Div => this.GenerateDiv(sb),
                Opcode.DivMod => this.GenerateDivMod(sb),
                Opcode.Mod => this.GenerateMod(sb),
                Opcode.Less => this.GenerateLess(sb),
                Opcode.LessOrEqual => this.GenerateLessOrEqual(sb),
                Opcode.Equal => this.GenerateEqual(sb),
                Opcode.Greater => this.GenerateGreater(sb),
                Opcode.GreaterOrEqual => this.GenerateGreaterOrEqual(sb),
                Opcode.Drop => this.GenerateDrop(sb),
                Opcode.Dup => this.GenerateDup(sb),
                Opcode.Over => this.GenerateOver(sb),
                Opcode.Swap => this.GenerateSwap(sb),
                Opcode.Rot => this.GenerateRot(sb),
                Opcode.Dump => this.GenerateDump(sb),
                Opcode.Emit => this.GenerateEmit(sb),
                _ => throw new ArgumentException($"Operation {op.Code} not supported")
            }
        };

    private StringBuilder GenerateIf(BlockOperation op, StringBuilder sb)
    {
        sb.AppendLine($"    pop rax");
        sb.AppendLine($"    test rax, rax");
        sb.AppendLine($"    jz ip{op.Label}");
        return sb;
    }

    private StringBuilder GenerateEnd(BlockOperation op, StringBuilder sb) =>
        sb.AppendLine($"ip{op.Label}:");

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

    private StringBuilder GeneratePush(StringBuilder sb, int value) =>
        sb.AppendLine($"    push {value}");
}
