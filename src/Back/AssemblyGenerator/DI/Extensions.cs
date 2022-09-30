namespace Back.AsssemblyGenerator.DI;

using Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    public static IServiceCollection AddAssemblyGenerator(this IServiceCollection services) =>
        services.AddTransient<Abstractions.IAssemblyGenerator, Core.AssemblyGenerator>();
}