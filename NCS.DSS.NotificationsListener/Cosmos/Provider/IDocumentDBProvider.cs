using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;

namespace NCS.DSS.NotificationsListener.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        //bool DoesCustomerResourceExist(Guid customerId);
        //Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        //bool DoesInteractionResourceExist(Guid interactionId);
        Task<ResourceResponse<Document>> CreateListenerNotificationAsync(Models.Notification noti);
    }
}