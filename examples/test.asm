segment .text
dump:
    mov r8, -3689348814741910323
    sub rsp, 40
    mov BYTE [rsp+20], 10
    lea rcx, [rsp+19]
.L2:
    mov rax, rdi
    mul r8
    mov rax, rdi
    shr rdx, 3
    lea rsi, [rdx+rdx*4]
    add rsi, rsi
    sub rax, rsi
    add eax, 48
    mov BYTE [rcx], al
    mov rax, rdi
    mov rdi, rdx
    mov rdx, rcx
    sub rcx, 1
    cmp rax, 9
    ja  .L2
    lea rax, [rsp+22]
    mov edi, 1
    mov rcx, rax
    sub rcx, rdx
    sub rdx, rax
    lea rsi, [rsp+22+rdx]
    mov rdx, rcx
    mov rax, 1
    syscall
    add rsp, 40
    ret
global _start
_start:
    ; *** PUSH ***
    push 21
    ; *** PUSH ***
    push 21
    ; *** PLUS ***
    pop rax
    pop rbx
    add rbx, rax
    push rbx
    ; *** DUMP ***
    pop rdi
    call dump
    mov rax, 60
    xor rdi, rdi
    syscall
