namespace Back;
using System.Text;

public class AssemblyGenerator
{
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
        sb.AppendLine("    mov rax, 1");
        sb.AppendLine("    syscall");
        sb.AppendLine("    add rsp, 40");
        sb.AppendLine("    ret");

        sb.AppendLine("global _start");
        sb.AppendLine("_start:");

        foreach (var op in operations)
            sb = this.Generate(op, sb);

        sb.AppendLine("    mov rax, 60");
        sb.AppendLine("    xor rdi, rdi");
        sb.AppendLine("    syscall");
        return sb.ToString();
    }
     
    private StringBuilder Generate(Operation op, StringBuilder sb)
    {
        sb.AppendLine($"    ; *** {op.Code} ***");
        sb = (op.Code, op.Value) switch
        {
            (Opcode.PUSH, int val) => this.GeneratePush(sb, val),
            (Opcode.PUSH, _) => throw new ArgumentException($"{op.location} Can only push integers"),
            (Opcode.PLUS, _) => this.GeneratePlus(sb),
            (Opcode.DUMP, _) => this.GenerateDump(sb),
            _ => throw new ArgumentException($"Operation ${op.Code} not supported")
        };
        return sb;
    }

    private StringBuilder GenerateDump(StringBuilder sb)
    {
        sb.AppendLine ("    pop rdi");
        sb.AppendLine ("    call dump");
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

    private StringBuilder GeneratePush(StringBuilder sb, int value) =>
        sb.AppendLine($"    push {value}");
}
