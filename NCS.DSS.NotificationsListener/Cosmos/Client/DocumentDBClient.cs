using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using System;

namespace NCS.DSS.NotificationsListener.Cosmos.Client
{
    public class DocumentDBClient : IDocumentDBClient
    {
        private DocumentClient _documentClient;
        private readonly IConfiguration _configuration;
        public DocumentDBClient(IConfiguration config)
        {
            _configuration = config;
        }

        public DocumentClient CreateDocumentClient()
        {
            if (_documentClient != null)
                return _documentClient;

            _documentClient = new DocumentClient(new Uri(
                _configuration.GetValue<string>("Endpoint")),
                _configuration.GetValue<string>("Key"));

            return _documentClient;
        }
    }
}
