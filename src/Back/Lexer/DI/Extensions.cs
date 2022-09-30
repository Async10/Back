namespace Back.Lexer.DI;

using Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    public static IServiceCollection AddLexer(this IServiceCollection services) =>
        services.AddTransient<Abstractions.ILexer, Core.Lexer>();
}