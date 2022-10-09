using Back.Shared.Abstractions;

namespace Back.Parser.Abstractions;

public record Operation(Opcode Code, Location Location);

public record IntOperation(Opcode Code, Location Location, int Value) : Operation(Code, Location);

public record EndOperation(Location Location, int EndAddress = -1) : Operation(Opcode.End, Location);

public record IfOperation(Location Location, int ElseOrEndAddress = -1) : Operation(Opcode.If, Location);

public record ElseOperation(Location Location, int ElseAddress = -1, int EndAddress = -1): Operation(Opcode.Else, Location);