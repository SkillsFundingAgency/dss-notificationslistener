using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.NotificationsListener.Cosmos.Client
{
    public interface IDocumentDBClient
    {
        DocumentClient CreateDocumentClient();
    }
}