using System.Net;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;

namespace alphonsefonctionapp
{
    public class Televerser
    {
        private readonly ILogger<Televerser> _logger;

        public Televerser(ILogger<Televerser> logger)
        {
            _logger = logger;
        }

        [Function("Televerserimages")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("UploadImage triggered.");

            if (!req.Headers.TryGetValues("Content-Type", out var contentTypes))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var contentType = contentTypes.First();
            var boundary = HeaderUtilities.RemoveQuotes(
                MediaTypeHeaderValue.Parse(contentType).Boundary
            ).Value;

            if (string.IsNullOrEmpty(boundary))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Missing multipart boundary.");
                return bad;
            }

            var reader = new MultipartReader(boundary, req.Body);

            MultipartSection? section;
            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                var hasFileContentDisposition =
                    ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition)
                    && contentDisposition.DispositionType.Equals("form-data")
                    && !string.IsNullOrEmpty(contentDisposition.FileName.Value);

                if (!hasFileContentDisposition)
                    continue;

                var fileName = contentDisposition.FileName.Value?.Trim('"');

                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                var blobServiceClient = new BlobServiceClient(connectionString);

                var containerClient = blobServiceClient.GetBlobContainerClient("images");
                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient(fileName);

                await blobClient.UploadAsync(section.Body, overwrite: true);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(new
                {
                    message = "Upload successful",
                    fileName = fileName
                });

                return response;
            }

            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteStringAsync("No file found in request.");
            return errorResponse;
        }
    }
}
