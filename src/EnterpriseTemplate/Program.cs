using EnterpriseTemplate.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EnterpriseTemplate;
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
        args ??= Enumerable.Empty<string>().ToArray();
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(config =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseConsoleLifetime(options=>options.SuppressStatusMessages = true)
            .ConfigureAppConfiguration((builder, context) =>
            {
                builder.HostingEnvironment.EnvironmentName = builder.Configuration["ASPNETCORE_ENVIROMENT"] ?? "Production";
                context.AddJsonFile("appsettings.json", false);
                context.AddJsonFile($"appsettings.{builder.HostingEnvironment.EnvironmentName}.json", true);
                context.AddEnvironmentVariables();
            })
            .ConfigureServices((builder, services) =>
            {
                services.Configure<EnterpriseTemplateContext>(builder.Configuration.GetSection("Job"));
                services.AddHostedService<Job>();
            });
       
        return host;
    }
}
