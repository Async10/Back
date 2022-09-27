namespace Back;

public record Operation(Opcode Code, Location location, int? Value = null);
