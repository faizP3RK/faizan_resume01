using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Company.Function
{
    public static class GetResumeCounter
    {
        [FunctionName("GetResumeCounter")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "%CosmosDBDatabase%",
                containerName: "%CosmosDBContainer%",
                Connection = "CosmosDBConnection",
                Id = "1",
                PartitionKey = "1")] Counter counter,
            [CosmosDB(
                databaseName: "%CosmosDBDatabase%",
                containerName: "%CosmosDBContainer%",
                Connection = "CosmosDBConnection",
                Id = "1",
                PartitionKey = "1")] IAsyncCollector<Counter> counterOutput,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            if (counter == null)
            {
                counter = new Counter { id = "1", PartitionKey = "1", Count = 0 };
            }

            counter.Count++;

            await counterOutput.AddAsync(counter);

            return new OkObjectResult(new { Count = counter.Count });
        }
    }

    
}