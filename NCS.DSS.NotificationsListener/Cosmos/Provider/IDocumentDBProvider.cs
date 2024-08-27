using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.NotificationsListener.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<ResourceResponse<Document>> CreateListenerNotificationAsync(Models.Notification noti);
    }
}