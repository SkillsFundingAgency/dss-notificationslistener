using DFC.HTTP.Standard;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.NotificationsListener;
using NCS.DSS.NotificationsListener.Cosmos.Helper;
using NCS.DSS.NotificationsListener.Util;
using System;
using System.IO;

[assembly: FunctionsStartup(typeof(Startup))]
namespace NCS.DSS.NotificationsListener
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)  // secrets go here. This file is excluded from source control.
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddLogging();
            builder.Services.AddTransient<IResourceHelper, ResourceHelper>();
            builder.Services.AddTransient<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
            builder.Services.AddTransient<IHttpRequestHelper, HttpRequestHelper>();
            builder.Services.AddTransient<IDocumentDBHelper, DocumentDBHelper>();
            builder.Services.AddTransient<ISaveNotificationToDatabase, SaveNotificationToDatabase>();
            builder.Services.AddSingleton<IConfiguration>(config);
        }
    }
}
