using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Company.Function
{
    public static class GetResumeCounter
    {
        private static CosmosClient cosmosClient;
        private static Container container;

        [FunctionName("GetResumeCounter")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            // Here is where the counter gets updated.
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Load configuration from local.settings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Get Cosmos DB settings from local.settings.json
            string cosmosEndpoint = config["CosmosDBEndpoint"];
            string cosmosKey = config["CosmosDBKey"];
            string databaseName = config["CosmosDBDatabase"];
            string containerName = config["CosmosDBContainer"];

            // Initialize CosmosClient and Container
            cosmosClient = new CosmosClient(cosmosEndpoint, cosmosKey);
            container = cosmosClient.GetContainer(databaseName, containerName);

            // Get the document by ID
            var counter = await GetCounterById("1", "1");

            if (counter == null)
            {
                counter = new Counter { id = "1", PartitionKey = "1", Count = 0 };
            }

            // Update the document
            counter.Count++;

            // Replace the document in Cosmos DB
            var response = await container.ReplaceItemAsync(counter, counter.id, new PartitionKey(counter.PartitionKey));

            // Return the count value as JSON
            return new OkObjectResult(new { Count = counter.Count });
        }

        private static async Task<Counter> GetCounterById(string id, string partitionKey)
        {
            try
            {
                var response = await container.ReadItemAsync<Counter>(id, new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
