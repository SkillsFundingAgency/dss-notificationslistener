using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NCS.DSS.NotificationsListener.Annotations;
using NCS.DSS.NotificationsListener.Cosmos.Helper;
using NCS.DSS.NotificationsListener.Util;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.NotificationsListener.URLEndpoint.Function
{
    public class NotificationsListenerHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly ISaveNotificationToDatabase _saveNotification;

        public NotificationsListenerHttpTrigger(IResourceHelper resourceHelper, IHttpRequestHelper httpRequestMessageHelper, IHttpResponseMessageHelper httpResponseMessageHelper, ISaveNotificationToDatabase saveNotifcation)
        {
            _resourceHelper = resourceHelper;
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _saveNotification = saveNotifcation;
        }


        [FunctionName("Post")]
        [ProducesResponseTypeAttribute(typeof(Models.Notification), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Notification Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Notification does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new transfer resource.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "URLEndpoint")] HttpRequest req, 
            ILogger log)
        {

            string noti;
            string bearer = string.Empty;

            Models.Notification notification;

            try
            {
                notification = await _httpRequestMessageHelper.GetResourceFromRequest<Models.Notification>(req);
            }
            catch (JsonException ex)
            {
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (notification == null)
                return _httpResponseMessageHelper.UnprocessableEntity();

            string authHeader = string.Empty;

            if (req.Headers.TryGetValue("Authorization", out StringValues authToken))
            {
                authHeader = authToken.First();
            }
            else
            {
                log.LogInformation("Authorization header error !");
            }

            if (notification.ResourceURL != null)
            {
                if (notification.ResourceURL.ToString().Contains("collections"))
                {
                    var lastIndexOf = notification.ResourceURL.ToString().LastIndexOf("/", StringComparison.Ordinal);
                    if (lastIndexOf != -1)
                    {
                        var collectionId = notification.ResourceURL.ToString().Substring(lastIndexOf + 1);

                       if(Guid.TryParse(collectionId, out var collectionGuid))
                           notification.CollectionId = collectionGuid;
                    }
                }
            }

            noti = "Customer Id : " + notification.CustomerId + Environment.NewLine +
                   "URL : " + notification.ResourceURL + Environment.NewLine +
                   "LastModifiedDate : " + notification.LastModifiedDate + Environment.NewLine +
                   "Touchpoint Id : " + notification.TouchpointId + Environment.NewLine +
                   "Collection Id : " + notification.CollectionId + Environment.NewLine +
                   "Bearer : " + authHeader;

            log.LogInformation(noti);

            await _saveNotification.SaveNotificationToDBAsync(notification);

            return _httpResponseMessageHelper.Ok(noti);
        }
    }
}