namespace Back.AsssemblyGenerator.Abstractions;

using Back.Parser.Abstractions;

public interface IAssemblyGenerator
{
    string Generate(IEnumerable<Operation> operations);
}
