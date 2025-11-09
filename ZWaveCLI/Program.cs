using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ZWaveCLI;

/// <summary>
/// Z-Wave CLI application entry point.
/// This is the main program that sets up dependency injection,
/// configures the command-line interface, and runs commands.
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        // Build the host with dependency injection
        var host = CreateHostBuilder(args).Build();

        // Get the root command from DI
        var rootCommand = host.Services.GetRequiredService<RootCommand>();

        // Execute the command
        return await rootCommand.InvokeAsync(args);
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Configure logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                // Register services
                // TODO: Register connection service, port detector, etc.

                // Register CLI commands
                services.AddSingleton<RootCommand>(sp =>
                {
                    var rootCommand = new RootCommand("Z-Wave PC Controller CLI - Manage Z-Wave networks from the command line");

                    // TODO: Add subcommands (connect, list-ports, info, etc.)

                    return rootCommand;
                });
            });
}
