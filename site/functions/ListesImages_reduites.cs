using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;

namespace alphonsefonctionapp
{
    public class ListImages_reduites
    {
        private readonly ILogger<ListImages_reduites> _logger;

        public ListImages_reduites(ILogger<ListImages_reduites> logger)
        {
            _logger = logger;
        }

        [Function("listalphonse")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ireduites")] HttpRequest req)
        {
            string? conn = System.Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            if (string.IsNullOrWhiteSpace(conn))
                return new ObjectResult("Missing AzureWebJobsStorage") { StatusCode = 500 };

            string? accountName = System.Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_NAME");
            if (string.IsNullOrWhiteSpace(accountName))
                return new ObjectResult("Missing STORAGE_ACCOUNT_NAME") { StatusCode = 500 };

            // Container des miniatures
            var containerClient = new BlobContainerClient(conn, "ireduites");

            // Base URL publique du container (si container public Blob)
            string baseUrl = $"https://{accountName}.blob.core.windows.net/ireduites/";

            var urls = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                urls.Add(baseUrl + blobItem.Name);
            }

            return new OkObjectResult(urls);
        }
    }
}
