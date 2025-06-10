using DFC.HTTP.Standard;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.NotificationsListener.Helpers;
using NCS.DSS.NotificationsListener.Models;
using NCS.DSS.NotificationsListener.Services;

namespace NCS.DSS.NotificationsListener
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureAppConfiguration(config =>
                {
                    config.SetBasePath(Environment.CurrentDirectory)
                        .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    services.AddOptions<NotificationsListenerConfigurationSettings>()
                        .Bind(configuration);

                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddLogging();

                    services.AddTransient<IHttpRequestHelper, HttpRequestHelper>();
                    services.AddTransient<IDynamicHelper, DynamicHelper>();

                    services.AddSingleton<ICosmosDBService, CosmosDBService>();
                    services.AddSingleton(sp =>
                    {
                        var config = sp.GetRequiredService<IOptions<NotificationsListenerConfigurationSettings>>().Value;
                        var options = new CosmosClientOptions()
                        {
                            ConnectionMode = ConnectionMode.Gateway
                        };

                        return new CosmosClient(config.Endpoint, config.Key, options);
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.Services.Configure<LoggerFilterOptions>(options =>
                    {
                        LoggerFilterRule? defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                        if (defaultRule is not null)
                        {
                            options.Rules.Remove(defaultRule);
                        }
                    });
                })
                .Build();
            await host.RunAsync();
        }
    }
}