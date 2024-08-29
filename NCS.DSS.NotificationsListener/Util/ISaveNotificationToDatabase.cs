namespace NCS.DSS.NotificationsListener.Util
{
    public interface ISaveNotificationToDatabase
    {
        Task SaveNotificationToDBAsync(Models.Notification notification);
    }
}
