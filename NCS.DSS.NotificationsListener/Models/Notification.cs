namespace NCS.DSS.NotificationsListener.Models
{
    public class Notification
    {
        /* The Microsoft.Azure.Cosmos SDK now requires a Document ID to be provided for all create requests:
           - https://github.com/Azure/azure-sdk-for-python/blob/0af8deab3958acfae24cae2e24060b576c71b076/sdk/cosmos/azure-cosmos/azure/cosmos/container.py#L500 
           - https://learn.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.container.createitemasync?view=azure-dotnet#parameters  
        */
        public string id { get; set; } = Guid.NewGuid().ToString();
        
        public Guid CustomerId { get; set; }
        public Uri ResourceURL { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string TouchpointId { get; set; }
        public Guid CollectionId { get; set; }
    }
}