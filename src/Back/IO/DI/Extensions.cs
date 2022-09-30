namespace Back.IO.DI;

using Back.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    public static IServiceCollection AddIO(
            this IServiceCollection services,
            Action<LoggerOptions>? configureOptions = null) =>
        services
            .AddTransient<Abstractions.IOutput, Core.Output>()
            .AddSingleton<Abstractions.ILogger, Core.Logger>(services =>
            {
                var output = services.GetRequiredService<IOutput>();
                var options = new LoggerOptions();
                configureOptions?.Invoke(options);
                return new Core.Logger(output)
                {
                    Quiet = options.Quiet,
                };
            })
            .AddTransient<Abstractions.IFileWriter, Core.FileWriter>()
            .AddTransient<Abstractions.ICommandRunner, Core.CommandRunner>();
}