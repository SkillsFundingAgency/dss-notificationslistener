
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using System;

namespace NCS.DSS.NotificationsListener.Cosmos.Helper
{
    public class DocumentDBHelper : IDocumentDBHelper
    {
        private readonly IConfiguration _configuration;
        private Uri _documentCollectionUri;
        private Uri _documentUri;
        private string _databaseId => _configuration.GetValue<string>("DatabaseId");
        private string _collectionId => _configuration.GetValue<string>("CollectionId");
        private string _customerDatabaseId => _configuration.GetValue<string>("CustomerDatabaseId");
        private string _customerCollectionId => _configuration.GetValue<string>("CustomerCollectionId");
        private Uri _interactionDocumentCollectionUri;
        private string _interactionDatabaseId => _configuration.GetValue<string>("InteractionDatabaseId");
        private string _interactionCollectionId => _configuration.GetValue<string>("InteractionCollectionId");
        private Uri _ListenerNotificationDocumentCollectionUri;
        private string _ListenerNotificationDatabaseId => _configuration.GetValue<string>("ListenerDatabaseId");
        private string _ListenerNotificationCollectionId => _configuration.GetValue<string>("ListenerCollectionId");
        private Uri _customerDocumentCollectionUri;

        public DocumentDBHelper(IConfiguration config)
        {
            _configuration = config;
        }

        public Uri CreateDocumentCollectionUri()
        {
            if (_documentCollectionUri != null)
                return _documentCollectionUri;

            _documentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _databaseId,
                _collectionId);

            return _documentCollectionUri;
        }
        
        public Uri CreateDocumentUri(Guid transferId)
        {
            if (_documentUri != null)
                return _documentUri;

            _documentUri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, transferId.ToString());

            return _documentUri;

        }

        #region CustomerDB

        public Uri CreateCustomerDocumentCollectionUri()
        {
            if (_customerDocumentCollectionUri != null)
                return _customerDocumentCollectionUri;

            _customerDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _customerDatabaseId, _customerCollectionId);

            return _customerDocumentCollectionUri;
        }

        #endregion

        #region InteractionDB

        public Uri CreateInteractionDocumentCollectionUri()
        {
            if (_interactionDocumentCollectionUri != null)
                return _interactionDocumentCollectionUri;

            _interactionDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _interactionDatabaseId, _interactionCollectionId);

            return _interactionDocumentCollectionUri;
        }

        #endregion

        #region ListenerNotificationDB

        public Uri CreateListenerNotificationDocumentCollectionUri()
        {
            if (_ListenerNotificationDocumentCollectionUri != null)
                return _ListenerNotificationDocumentCollectionUri;

            _ListenerNotificationDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _ListenerNotificationDatabaseId, _ListenerNotificationCollectionId);

            return _ListenerNotificationDocumentCollectionUri;
        }

        #endregion   


    }
}
