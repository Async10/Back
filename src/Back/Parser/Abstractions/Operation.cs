using Back.Shared.Abstractions;

namespace Back.Parser.Abstractions;

public record Operation(Opcode Code, Location Location);

public record IntOperation(Opcode Code, Location Location, int Value) : Operation(Code, Location);

public record BlockOperation(Opcode Code, Location Location, int Label) : Operation(Code, Location);
