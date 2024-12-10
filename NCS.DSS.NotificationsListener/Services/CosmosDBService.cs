using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.NotificationsListener.Models;

namespace NCS.DSS.NotificationsListener.Services
{
    public class CosmosDBService : ICosmosDBService
    {
        private readonly Container _container;
        private readonly ILogger<CosmosDBService> _logger;

        public CosmosDBService(CosmosClient cosmosClient, IOptions<NotificationsListenerConfigurationSettings> configOptions, ILogger<CosmosDBService> logger)
        {
            var config = configOptions.Value;

            _container = GetContainer(cosmosClient, config.ListenerDatabaseId, config.ListenerCollectionId);
            _logger = logger;
        }
        private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId) 
            => cosmosClient.GetContainer(databaseId, collectionId);

        public async Task<ItemResponse<Notification>> CreateNewNotificationDocument(Notification newDocument)
        {
            _logger.LogInformation($"{nameof(CreateNewNotificationDocument)} function has been invoked");

            _logger.LogInformation("Attempting to create new document in Cosmos DB");

            ItemResponse<Notification> createRequestResponse = await _container.CreateItemAsync(newDocument, PartitionKey.None);

            _logger.LogInformation($"{nameof(CreateNewNotificationDocument)} function has finished invocation");

            return createRequestResponse;
        }
    }
}
