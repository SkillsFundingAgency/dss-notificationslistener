namespace NCS.DSS.NotificationsListener.Cosmos.Helper
{
    public interface IDocumentDBHelper
    {
        //Uri CreateDocumentCollectionUri();
        //Uri CreateDocumentUri(Guid transferId);
        //Uri CreateCustomerDocumentCollectionUri();
        //Uri CreateInteractionDocumentCollectionUri();
        Uri CreateListenerNotificationDocumentCollectionUri();
    }
}