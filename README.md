# Azure Static Web App + Blazor + Distributed cache with Cosmos DB

This template contains an example [Blazor WebAssembly](https://docs.microsoft.com/aspnet/core/blazor/?view=aspnetcore-3.1#blazor-webassembly) client application, a C# [Azure Functions](https://docs.microsoft.com/azure/azure-functions/functions-overview) and a C# class library with shared code that leverages **Azure Cosmos DB as a distributed cache**.

## Structure

The structure of the project is based off https://github.com/staticwebdev/blazor-starter:

- **Client**: The Blazor WebAssembly sample application
- **API**: A C# Azure Functions API, which the Blazor application will call
- **Shared**: A C# class library with a shared data model between the Blazor and Functions application

## Dependencies

In order to easily use Cosmos DB as a distributed cache provider, this project uses the [Microsoft.Extensions.Caching.Cosmos](https://github.com/Azure/Microsoft.Extensions.Caching.Cosmos) package.

## Relevant code sections

Since the API is just an Azure Functions project, we can leverage [Azure Functions Dependency Injection](https://docs.microsoft.com/azure/azure-functions/functions-dotnet-dependency-injection#use-injected-dependencies) to register the cache dependency:

```csharp
public override void Configure(IFunctionsHostBuilder builder)
{
    builder.Services.AddCosmosCache((CosmosCacheOptions cacheOptions) =>
    {
        cacheOptions.ContainerName = "myCacheContainer";
        cacheOptions.DatabaseName = "myCacheDatabase";
        cacheOptions.ClientBuilder = new CosmosClientBuilder(configuration["CosmosDBConnectionString"]);
        cacheOptions.CreateIfNotExists = true;
    });
}
```

For the full registration code, see [Startup.cs](https://github.com/ealsur/staticblazorcache/blob/main/Api/Startup.cs)

Once the cache is registered, it can be used in any of the APIs by leveraging dependency injection:

```csharp
private readonly IDistributedCache cache;

public CacheFunction(IDistributedCache cache)
{
    this.cache = cache;
}

[FunctionName("Cache")]
public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
    ILogger log)
{
    // Is there a cached value?
    string cachedValue = await this.cache.GetStringAsync("myCacheKey");
    // ...
```

For the full code, see [CacheFunction.cs](https://github.com/ealsur/staticblazorcache/blob/main/Api/CacheFunction.cs).

## Why?

Why would you want to have a **distributed cache that is backed by a fast key/value store with global replication**?
Since the APIs are serverless, you cannot rely on in-memory objects for cache, your APIs can be scaling across multiple instances in a serverless environment, if you have costly data that you want to cache, you need to store it externally.

With Cosmos DB's global replication and low latency support, you can build static web apps that span multiple regions and cache data locally to provide users the fastest possible experience.

For data consistency, please read the [Microsoft.Extensions.Caching.Cosmos documentation](https://github.com/Azure/Microsoft.Extensions.Caching.Cosmos#cosmos-account-consistency).

## Deploy to Azure Static Web Apps

This application can be deployed to [Azure Static Web Apps](https://docs.microsoft.com/azure/static-web-apps), to learn how, check out [our quickstart guide](https://aka.ms/blazor-swa/quickstart).

Make sure to create a Configuration setting called `CosmosDBConnectionString` with the connection string to an existing Azure Cosmos DB account.
