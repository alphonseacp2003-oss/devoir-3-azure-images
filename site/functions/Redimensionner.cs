using System.IO;
using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace alphonsefonctionapp
{
    public class Redimensionner
    {
        private readonly ILogger<Redimensionner> _logger;

        public Redimensionner(ILogger<Redimensionner> logger)
        {
            _logger = logger;
        }

        [Function("Redimensionnerimages")]
        public async Task Run(
            [BlobTrigger("images/{name}", Connection = "AzureWebJobsStorage")] Stream imageStream,
            string name)
        {
            _logger.LogInformation($"Redimensionnerimages triggered for: {name}");

            using var image = await Image.LoadAsync(imageStream);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(256, 256),
                Mode = ResizeMode.Crop
            }));

            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var blobServiceClient = new BlobServiceClient(connectionString);

            var ireduitesContainer = blobServiceClient.GetBlobContainerClient("ireduites");
            await ireduitesContainer.CreateIfNotExistsAsync();

            var ireduitesBlob = ireduitesContainer.GetBlobClient(name);

            using var outputStream = new MemoryStream();
            await image.SaveAsJpegAsync(outputStream);
            outputStream.Position = 0;

            await ireduitesBlob.UploadAsync(
              outputStream,
              new BlobUploadOptions
              {
                  HttpHeaders = new BlobHttpHeaders
                  {
                      ContentType = "image/jpeg"
                  }
              }
            );


            _logger.LogInformation($"ireduites created: {name}");
        }
    }
}
