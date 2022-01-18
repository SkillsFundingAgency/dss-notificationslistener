using Microsoft.Extensions.Configuration;
using NCS.DSS.NotificationsListener.Cosmos.Provider;
using System.Threading.Tasks;

namespace NCS.DSS.NotificationsListener.Util
{
    public class SaveNotificationToDatabase : ISaveNotificationToDatabase
    {
        public IConfiguration _configuration;

        public SaveNotificationToDatabase(IConfiguration config)
        {
            _configuration = config;
        }

        public async Task SaveNotificationToDBAsync(Models.Notification notification)
        {
            var documentDbProvider = new DocumentDBProvider(_configuration);
            await documentDbProvider.CreateListenerNotificationAsync(notification);
        }
    }
}
