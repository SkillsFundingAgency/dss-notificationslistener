using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using NCS.DSS.NotificationsListener.Cosmos.Client;
using NCS.DSS.NotificationsListener.Cosmos.Helper;

namespace NCS.DSS.NotificationsListener.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly DocumentDBHelper _documentDbHelper;
        private readonly DocumentDBClient _databaseClient;

        public DocumentDBProvider(IConfiguration config)
        {
            _documentDbHelper = new DocumentDBHelper(config);
            _databaseClient = new DocumentDBClient(config);
        }

        public async Task<ResourceResponse<Document>> CreateListenerNotificationAsync(Models.Notification noti)
        {
            var collectionUri = _documentDbHelper.CreateListenerNotificationDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, noti);

            return response;
        }
    }
}