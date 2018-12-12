using NCS.DSS.NotificationsListener.Cosmos.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.NotificationsListener.Util
{
    class SaveNotificationToDatabase
    {
        public static async Task SaveNotificationToDBAsync(Models.Notification notification)
        {
            var documentDbProvider = new DocumentDBProvider();
            await documentDbProvider.CreateListenerNotificationAsync(notification);
        }

    }
}
