using Microsoft.Azure.Cosmos;
using NCS.DSS.NotificationsListener.Models;

namespace NCS.DSS.NotificationsListener.Services
{
    public interface ICosmosDBService
    {
        Task<ItemResponse<Notification?>> CreateNewNotificationDocument(Notification newDocument);
    }
}
