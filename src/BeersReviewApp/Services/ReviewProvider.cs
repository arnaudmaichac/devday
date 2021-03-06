﻿using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using BeersReviewApp.Models;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BeersReviewApp.Services
{
    public class ReviewProvider
    {
        private readonly string storageAccountConnectionString;
        private readonly string tableStorageAccountConnectionString;

        private readonly string blobContainerName;
        private readonly string queueName;

        private readonly CloudTable table;

        public ReviewProvider(IConfiguration configuration)
        {
            storageAccountConnectionString = configuration.GetValue<string>("storageAccountConnectionString");
            tableStorageAccountConnectionString = configuration.GetValue<string>("tableStorageAccountConnectionString");

            var tableStorageAccount = CloudStorageAccount.Parse(tableStorageAccountConnectionString);
            CloudTableClient tableClient = tableStorageAccount.CreateCloudTableClient(new TableClientConfiguration());

            string tableName = configuration.GetValue<string>("tableName");
            table = tableClient.GetTableReference(tableName);

            blobContainerName = configuration.GetValue<string>("blobContainerName");
            queueName = configuration.GetValue<string>("queueName");
        }

        public IEnumerable<BeerReview> GetReviewsAsync()
        {
            return table.ExecuteQuery(new TableQuery<BeerReview>());
        }

        public async Task<BeerReview> GetReviewAsync(Guid id)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<BeerReview>("Reviews", id.ToString());
            TableResult result = await table.ExecuteAsync(retrieveOperation);

            return result.Result as BeerReview;
        }

        public async Task<Guid> CreateReviewAsync(Stream image, string reviewText)
        {
            var recordId = Guid.NewGuid();

            // save image
            var blobContainerClient = new BlobContainerClient(storageAccountConnectionString, blobContainerName);
            await blobContainerClient.CreateIfNotExistsAsync(publicAccessType: Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            var blockBlob = blobContainerClient.GetBlobClient(recordId.ToString());
            await blockBlob.UploadAsync(image);

            // save review
            await table.CreateIfNotExistsAsync();

            TableOperation operation = TableOperation.Insert(new BeerReview
            {
                RowKey = recordId.ToString(),
                MediaUrl = blockBlob.Uri.ToString(),
                ReviewText = reviewText,
                IsApproved = null,
                Created = DateTime.UtcNow
            });

            await table.ExecuteAsync(operation);

            // notify through queue
            var queueClient = new QueueClient(storageAccountConnectionString, queueName);
            await queueClient.CreateIfNotExistsAsync();

            // warning: there is an issue with the Azure.Storage.Queues v12 NuGet package
            // A JSON payload has to be encoded in base64
            // https://github.com/Azure/azure-sdk-for-net/issues/10242
            var payload = new { BlobName = recordId.ToString(), DocumentId = recordId.ToString() };
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
            await queueClient.SendMessageAsync(Convert.ToBase64String(plainTextBytes));

            return recordId;
        }
    }
}