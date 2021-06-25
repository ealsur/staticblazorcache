using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System.Threading.Tasks;
using BlazorApp.Shared;

namespace BlazorApp.Api
{
    public class CacheFunction
    {
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
            if (cachedValue == null)
            {
                log.LogInformation("Did not find cache");
                // Doesn't exist
                cachedValue = DateTime.UtcNow.ToString();

                // Define expiration
                DistributedCacheEntryOptions expireIn5Minutes = new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };
                await this.cache.SetStringAsync("myCacheKey", cachedValue, expireIn5Minutes);

                // Inform the caller the value did not come from cache
                return new OkObjectResult(new CacheData() { Cached = false, Value = cachedValue });
            }

            log.LogInformation("Cache hit");
            // Inform the caller the value came from cache
            return new OkObjectResult(new CacheData() { Cached = true, Value = cachedValue });
        }
    }
}
