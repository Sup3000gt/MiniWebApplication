using Microsoft.Azure.Cosmos;
using MiniWebApplication.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniWebApplication.Services
{
    public class CosmosDbService
    {
        private readonly Container _container;

        public CosmosDbService(CosmosClient client, string databaseName, string containerName)
        {
            _container = client.GetContainer(databaseName, containerName);
        }

        public async Task AddReviewAsync(Review review)
        {
            review.Id = Guid.NewGuid().ToString(); // Ensure a unique ID
            review.Timestamp = DateTime.UtcNow;    // Set the timestamp
            await _container.CreateItemAsync(review, new PartitionKey(review.ProductId));
        }

        public async Task<List<Review>> GetReviewsByProductIdAsync(int productId)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.ProductId = @productId")
                            .WithParameter("@productId", productId);

            var iterator = _container.GetItemQueryIterator<Review>(query, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(productId)  // Ensure partition key is used
            });

            var reviews = new List<Review>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                reviews.AddRange(response.ToList());
            }

            return reviews;
        }

    }
}
