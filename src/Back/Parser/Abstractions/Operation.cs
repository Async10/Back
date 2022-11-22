using Back.Shared.Abstractions;

namespace Back.Parser.Abstractions;

public record Operation(Opcode Code, Location Location);

public record LongOperation(Opcode Code, Location Location, long Value) : Operation(Code, Location);

public record StringOperation(Opcode Code, Location Location, string Value) : Operation(Code, Location);

public record EndOperation(Location Location, int EndAddress = -1, int BeginAddress = -1) : Operation(Opcode.End, Location);

public record IfOperation(Location Location, int ElseOrEndAddress = -1) : Operation(Opcode.If, Location);

public record ElseOperation(Location Location, int ElseAddress = -1, int EndAddress = -1): Operation(Opcode.Else, Location);

public record BeginOperation(Location Location, int BeginAddress = -1): Operation(Opcode.Begin, Location);

public record WhileOperation(Location Location, int EndAddress = -1): Operation(Opcode.End, Location);

public record ProcOperation(Location Location, string ProcName, int ProcAddress = -1, int ReturnAddress = -1): Operation(Opcode.Proc, Location);

public record CallOperation(Location Location, string ProcName, int ProcAddress = -1, int CallAddress = -1): Operation(Opcode.Call, Location);

public record ReturnOperation(Location Location, int ReturnAddress = -1): Operation(Opcode.Return, Location);