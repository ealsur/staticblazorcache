using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(BlazorApp.Api.Startup))]

namespace BlazorApp.Api
{
    public class Startup : FunctionsStartup
    {
        private static IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                // Loading the config, can be renamed to any file or any config source
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddCosmosCache((CosmosCacheOptions cacheOptions) =>
            {
                cacheOptions.ContainerName = "myCacheContainer";
                cacheOptions.DatabaseName = "myCacheDatabase";
                cacheOptions.ClientBuilder = new CosmosClientBuilder(configuration["CosmosDBConnectionString"])
                    .WithConnectionModeGateway(); // See https://docs.microsoft.com/azure/cosmos-db/sql-sdk-connection-modes
                cacheOptions.CreateIfNotExists = true;
            });

        }
    }
}