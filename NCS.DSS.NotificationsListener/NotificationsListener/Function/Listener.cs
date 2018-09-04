using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.NotificationsListener.Models;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace NCS.DSS.NotificationsListener.NotificationsListener.Function
{
    public static class Listener
    {
        [FunctionName("Listener")]
        public static void Run([ServiceBusTrigger("eastandbucks", "eastandbucks", AccessRights.Listen, Connection = "ServiceBusConnectionString")]BrokeredMessage serviceBusMessage, ILogger log)
        {
            var body = new StreamReader(serviceBusMessage.GetBody<Stream>(), Encoding.UTF8).ReadToEnd();

            var customer = JsonConvert.DeserializeObject<MessageModel>(body);

            if (customer == null)
                return;

            var serialised = JsonConvert.SerializeObject(customer);
            log.LogInformation(serialised);
        }
    }
}
