namespace NCS.DSS.NotificationsListener.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        bool DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        bool DoesInteractionResourceExist(Guid interactionId);
    }
}