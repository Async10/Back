namespace Back.Compiler.Abstractions;

using Back.Shared.Abstractions;

public interface ICompiler
{
    bool TryCompile(SourceFile file, bool run);
}
