namespace Back.Parser.DI;

using Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    public static IServiceCollection AddParser(this IServiceCollection services) =>
        services.AddTransient<Abstractions.IParser, Core.Parser>();
}