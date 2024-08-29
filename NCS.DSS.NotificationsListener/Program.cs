using DFC.HTTP.Standard;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCS.DSS.NotificationsListener.Cosmos.Helper;
using NCS.DSS.NotificationsListener.Cosmos.Provider;
using NCS.DSS.NotificationsListener.Util;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(config =>
    {
        config.SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("local.settings.json", optional: true,
                reloadOnChange: false) // secrets go here. This file is excluded from source control.
            .AddEnvironmentVariables();
    })
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddLogging();
        services.AddTransient<IHttpRequestHelper, HttpRequestHelper>();
        services.AddTransient<IDocumentDBHelper, DocumentDBHelper>();
        services.AddTransient<ISaveNotificationToDatabase, SaveNotificationToDatabase>();
        services.AddTransient<IDocumentDBProvider, DocumentDBProvider>();
        services.AddTransient<IDynamicHelper, DynamicHelper>();
    })
    .ConfigureLogging(logging =>
    {
        // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
        // For more information, see https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#application-insights
        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            LoggerFilterRule defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
    })
    .Build();

host.Run();


