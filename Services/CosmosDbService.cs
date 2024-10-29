using Azure.Core.Pipeline;
using Microsoft.Azure.Cosmos;
using MiniWebApplication.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using System;


namespace MiniWebApplication.Services
{
    public class CosmosDbService
    {
        private readonly Container _container;
        private readonly ILogger<CosmosDbService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public CosmosDbService(CosmosClient client, string databaseName, string containerName, ILogger<CosmosDbService> logger)
        {
            _container = client.GetContainer(databaseName, containerName);
            _logger = logger;

            // Log CosmosClient connection mode to confirm Direct mode
            _logger.LogInformation("CosmosClient configured with ConnectionMode: {Mode}",
                                   client.ClientOptions.ConnectionMode);

            _retryPolicy = Policy
                .Handle<CosmosException>(ex =>
                    ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                    ex.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
                    ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), // Exponential backoff
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        // Log the retry attempt
                        _logger.LogWarning("Executing retry {RetryCount} after {TimeSpan} due to {ExceptionMessage}",
                            retryCount, timeSpan, exception.Message);
                    });
        }


        public async Task AddReviewAsync(Review review)
        {
            review.Id = Guid.NewGuid().ToString(); // Ensure a unique ID
            review.Timestamp = DateTime.UtcNow;    // Set the timestamp
            await _container.CreateItemAsync(review, new PartitionKey(review.ProductId));
        }

        public async Task<List<Review>> GetReviewsByProductIdAsync(int productId)
        {
            var startTime = DateTime.UtcNow;
            var query = new QueryDefinition(
                "SELECT c.id, c.UserName, c.Rating, c.Comment, c.Timestamp FROM c WHERE c.ProductId = @productId ORDER BY c.Timestamp DESC")
                .WithParameter("@productId", productId);


            var iterator = _container.GetItemQueryIterator<Review>(
                query,
                requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(productId),
                    MaxItemCount = 20
                });

            var stopwatch = Stopwatch.StartNew();
            var reviews = new List<Review>();

            while (iterator.HasMoreResults)
            {
                var response = await _retryPolicy.ExecuteAsync(() => iterator.ReadNextAsync());
                reviews.AddRange(response.ToList());

                _logger.LogInformation($"Page fetched in {response.RequestCharge} RUs.");
                _logger.LogInformation("ReadNextAsync Diagnostics: {Diagnostics}", response.Diagnostics.GetClientElapsedTime());
            }
            stopwatch.Stop();
            _logger.LogInformation("Total method execution time: {Time} ms", stopwatch.Elapsed.TotalMilliseconds);

            return reviews;

        }
    }
}
