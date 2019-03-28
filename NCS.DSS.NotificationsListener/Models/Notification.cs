using System;

namespace NCS.DSS.NotificationsListener.Models
{
    public class Notification
    {
        public Guid CustomerId { get; set; }
        public Uri ResourceURL { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string TouchpointId { get; set; }
        public Guid CollectionId { get; set; }
    }
}