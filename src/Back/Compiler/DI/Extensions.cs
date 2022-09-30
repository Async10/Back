namespace Back.Compiler.DI;

using Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    public static IServiceCollection AddCompiler(this IServiceCollection services) =>
        services.AddTransient<Abstractions.ICompiler, Core.Compiler>();
}