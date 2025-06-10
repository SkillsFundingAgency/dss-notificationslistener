namespace NCS.DSS.NotificationsListener.Models
{
    public class NotificationsListenerConfigurationSettings
    {
        public required string Endpoint { get; set; }
        public required string Key { get; set; }
        public required string ListenerDatabaseId { get; set; }
        public required string ListenerCollectionId { get; set; }
    }
}
