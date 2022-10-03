using Back.Shared.Abstractions;

namespace Back.Parser.Abstractions;

public record Operation(Opcode Code, Location Location);

public record IntOperation(Opcode Code, Location Location, int Value) : Operation(Code, Location);

public record LabelOperation(Opcode Code, Location Location, int Label) : Operation(Code, Location);

public record JumpOperation(Opcode Code, Location Location, int Address) : Operation(Code, Location);

public record JumpLabelOperation(Opcode Code, Location Location, int Address, int Label): Operation(Code, Location);