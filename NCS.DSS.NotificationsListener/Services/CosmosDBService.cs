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

        public async Task<ItemResponse<Notification?>> CreateNewNotificationDocument(Notification newDocument)
        {
            _logger.LogInformation("Starting {MethodName}", nameof(CreateNewNotificationDocument));
        
            try
            {
                _logger.LogInformation("Attempting to create new document in Cosmos DB. ID: {DocumentId}", newDocument?.id);
                var response = await _container.CreateItemAsync(newDocument, PartitionKey.None);
                _logger.LogInformation("Successfully created a new document in Cosmos DB. ID: {DocumentId}", newDocument?.id);
                return response;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Failed to create document in Cosmos DB. ID: {DocumentId}. Exception: {ErrorMessage}", newDocument?.id, ex.Message);
                throw;
            }
            finally
            {
                _logger.LogInformation("Finished {MethodName}", nameof(CreateNewNotificationDocument));
            }
        }
    }
}
