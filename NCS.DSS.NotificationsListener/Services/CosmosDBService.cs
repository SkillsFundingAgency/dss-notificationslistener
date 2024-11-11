using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using NCS.DSS.NotificationsListener.Models;

namespace NCS.DSS.NotificationsListener.Services
{
    public class CosmosDBService : ICosmosDBService
    {
        private readonly CosmosClient _cosmosDbClient;
        private readonly ILogger<CosmosDBService> _logger;
        private readonly string databaseName = Environment.GetEnvironmentVariable("ListenerDatabaseId");
        private readonly string containerName = Environment.GetEnvironmentVariable("ListenerCollectionId");

        public CosmosDBService(CosmosClient cosmosClient, ILogger<CosmosDBService> logger)
        {
            _cosmosDbClient = cosmosClient;
            _logger = logger;
        }

        public async Task<ItemResponse<Notification>> CreateNewNotificationDocument(Notification newDocument)
        {
            _logger.LogInformation($"{nameof(CreateNewNotificationDocument)} function has been invoked");
            Container cosmosDbContainer = _cosmosDbClient.GetContainer(databaseName, containerName);

            _logger.LogInformation("Attempting to create new document in Cosmos DB");

            ItemResponse<Notification> createRequestResponse = await cosmosDbContainer.CreateItemAsync(newDocument, PartitionKey.None);

            _logger.LogInformation($"{nameof(CreateNewNotificationDocument)} function has finished invocation");

            return createRequestResponse;
        }
    }
}
