using EnterpriseTemplate.Models;
using EnterpriseTemplate.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace EnterpriseTemplate;
[ExcludeFromCodeCoverage]
class Program
{
    public static async Task Main(string[] args)
    {
        using var cancellationSource = new CancellationTokenSource();
        var builder = GetHostBuilder(args);
        await builder.Build().RunAsync(cancellationSource.Token);
    }
    internal static IHostBuilder GetHostBuilder(string[]? args)
    {
        var switchMappings = new Dictionary<string, string>()
        {
            { "-v", "Logging:LogLevel:Default" },
            { "--verbosity", "Logging:LogLevel:Default" },
            { "-n", "Job:Name" },
            { "--name", "Job:Name" }
        };

        args ??= Enumerable.Empty<string>().ToArray();
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(config =>
            {
                config.SetBasePath(AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .UseContentRoot(AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory)
            .UseConsoleLifetime(options=>options.SuppressStatusMessages = true)
            .ConfigureAppConfiguration((builder, context) =>
            {
                builder.HostingEnvironment.EnvironmentName = builder.Configuration["ASPNETCORE_ENVIROMENT"] ?? "Production";
                context.AddJsonFile("appsettings.json", false);
                context.AddJsonFile($"appsettings.{builder.HostingEnvironment.EnvironmentName}.json", true);
                context.AddEnvironmentVariables();
                context.AddCommandLine(args, switchMappings);
            })
            .ConfigureLogging((builder, logging) =>
            {
                logging.ClearProviders();
                logging.AddDebug();
                logging.AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Disabled;
                });
                logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
            })
            .ConfigureServices((builder, services) =>
            {
                services.Configure<EnterpriseTemplateContext>(builder.Configuration.GetSection("Job"));
                services.AddSingleton<IDateService, DateService>();
                services.AddHostedService<Job>();
            });
       
        return host;
    }
}
