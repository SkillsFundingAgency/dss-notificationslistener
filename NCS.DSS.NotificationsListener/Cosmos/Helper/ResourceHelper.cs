using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NCS.DSS.NotificationsListener.Cosmos.Provider;

namespace NCS.DSS.NotificationsListener.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        private readonly IConfiguration _configuration;
        public ResourceHelper(IConfiguration config)
        {
            _configuration = config;
        }

        public bool DoesCustomerExist(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider(_configuration);
            var doesCustomerExist = documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider(_configuration);
            var isCustomerReadOnly = await documentDbProvider.DoesCustomerHaveATerminationDate(customerId);

            return isCustomerReadOnly;
        }

        public bool DoesInteractionExist(Guid interactionId)
        {
            var documentDbProvider = new DocumentDBProvider(_configuration);
            var doesInteractionExist = documentDbProvider.DoesInteractionResourceExist(interactionId);

            return doesInteractionExist;
        }
    }
}
