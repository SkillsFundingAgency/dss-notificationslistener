
using System;
using System.Configuration;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.NotificationsListener.Cosmos.Helper
{
    public class DocumentDBHelper : IDocumentDBHelper
    {
        private Uri _documentCollectionUri;
        private Uri _documentUri;
        private readonly string _databaseId = ConfigurationManager.AppSettings["DatabaseId"];
        private readonly string _collectionId = ConfigurationManager.AppSettings["CollectionId"];

        private Uri _customerDocumentCollectionUri;
        private readonly string _customerDatabaseId = ConfigurationManager.AppSettings["CustomerDatabaseId"];
        private readonly string _customerCollectionId = ConfigurationManager.AppSettings["CustomerCollectionId"];

        private Uri _interactionDocumentCollectionUri;
        private readonly string _interactionDatabaseId = ConfigurationManager.AppSettings["InteractionDatabaseId"];
        private readonly string _interactionCollectionId = ConfigurationManager.AppSettings["InteractionCollectionId"];

        private Uri _ListenerNotificationDocumentCollectionUri;
        private readonly string _ListenerNotificationDatabaseId = ConfigurationManager.AppSettings["ListenerDatabaseId"];
        private readonly string _ListenerNotificationCollectionId = ConfigurationManager.AppSettings["ListenerCollectionId"];


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
