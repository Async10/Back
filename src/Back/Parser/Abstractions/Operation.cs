using Back.Shared.Abstractions;

namespace Back.Parser.Abstractions;

public record Operation(Opcode Code, Location location, int? Value = null);
