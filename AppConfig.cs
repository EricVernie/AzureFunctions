using Azure.Identity;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace FnAppConfig
{
    public static class AppConfig
    {
        [FunctionName("GetAppConfig")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string key = req.Query["key"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            key = key ?? data?.name;
            var builder = new ConfigurationBuilder();
            //builder.AddAzureAppConfiguration(Environment.GetEnvironmentVariable("AppConfigKey"));
            var appConfigEndpoint=Environment.GetEnvironmentVariable("AppConfigKey");

            builder.AddAzureAppConfiguration(options =>
                    options.Connect(new Uri(appConfigEndpoint),
                                    new ManagedIdentityCredential()));

            IConfigurationRoot config = null;
            try
            {
                config = builder.Build();
            }
            catch(Exception ex)
            {
                
                return new BadRequestObjectResult(ex.Message);
            }
            



            return key != null
                ? (ActionResult)new OkObjectResult($"Value : {config[key]}")
                : new BadRequestObjectResult("Please pass a key on the query string or in the request body");
        }
    }
}
