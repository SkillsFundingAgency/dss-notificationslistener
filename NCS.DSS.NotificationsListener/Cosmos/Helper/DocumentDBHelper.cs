
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;

namespace NCS.DSS.NotificationsListener.Cosmos.Helper
{
    public class DocumentDBHelper : IDocumentDBHelper
    {
        private readonly IConfiguration _configuration;
        private Uri _ListenerNotificationDocumentCollectionUri;
        private string _ListenerNotificationDatabaseId => _configuration.GetValue<string>("ListenerDatabaseId");
        private string _ListenerNotificationCollectionId => _configuration.GetValue<string>("ListenerCollectionId");

        public DocumentDBHelper(IConfiguration config)
        {
            _configuration = config;
        }

        public Uri CreateListenerNotificationDocumentCollectionUri()
        {
            if (_ListenerNotificationDocumentCollectionUri != null)
                return _ListenerNotificationDocumentCollectionUri;

            _ListenerNotificationDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _ListenerNotificationDatabaseId, _ListenerNotificationCollectionId);

            return _ListenerNotificationDocumentCollectionUri;
        }
    }
}
