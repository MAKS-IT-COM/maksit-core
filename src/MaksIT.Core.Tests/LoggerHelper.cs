using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MaksIT.Core.Logging;

namespace MaksIT.Core.Tests;

/// <summary>
/// Provides helper methods for creating loggers in tests.
/// </summary>
public static class LoggerHelper
{
    /// <summary>
    /// Creates a console logger for testing purposes.
    /// </summary>
    /// <returns>An instance of <see cref="ILogger"/> configured for console logging.</returns>
    public static ILogger CreateConsoleLogger()
    {
        var serviceCollection = new ServiceCollection();

        // Use the reusable TestHostEnvironment for testing
        serviceCollection.AddSingleton<IHostEnvironment>(sp =>
            new TestHostEnvironment
            {
                EnvironmentName = Environments.Development,
                ApplicationName = "TestApp",
                ContentRootPath = Directory.GetCurrentDirectory()
            });

        serviceCollection.AddLogging(builder =>
        {
            var env = serviceCollection.BuildServiceProvider().GetRequiredService<IHostEnvironment>();
            builder.ClearProviders();
            builder.AddConsole(env);
        });

        var provider = serviceCollection.BuildServiceProvider();
        var factory = provider.GetRequiredService<ILoggerFactory>();
        return factory.CreateLogger("TestLogger");
    }
}