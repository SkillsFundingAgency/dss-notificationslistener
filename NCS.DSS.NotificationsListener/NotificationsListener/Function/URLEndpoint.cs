using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.NotificationsListener.Annotations;
using NCS.DSS.NotificationsListener.Cosmos.Helper;
using NCS.DSS.NotificationsListener.Helpers;
using NCS.DSS.NotificationsListener.Ioc;
using Newtonsoft.Json;
using Microsoft.Extensions.Primitives;
using System.Text;
using System.Collections.Generic;

namespace NCS.DSS.NotificationsListener.URLEndpoint.Function
{
    public static class NotificationsListenerHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Notification))]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Notification Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Notification does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new transfer resource.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "URLEndpoint")]HttpRequestMessage req, 
            ILogger log, [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper)
        {

            string noti;
            string bearer = string.Empty;

            Models.Notification notification;

            try
            {
                notification = await httpRequestMessageHelper.GetMessageFromRequest<Models.Notification>(req);
            }
            catch (JsonException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (notification == null)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(req);
            }
            else
            {

                string authHeader = string.Empty;

                if (req.Headers.TryGetValues("Authorization", out IEnumerable<string> authToken))
                {
                    authHeader = authToken.First();
                }
                else
                {
                    log.LogInformation("Authorization header error !");
                }


                noti = "Customer Id : " + notification.CustomerId + Environment.NewLine +
                       "URL : " + notification.ResourceURL + Environment.NewLine +
                       "LastModifiedDate : " + notification.LastModifiedDate.ToString() +
                       "Bearer : " + authHeader;

                log.LogInformation(noti);
            }

            return notification == null ? HttpResponseMessageHelper.BadRequest() :
                HttpResponseMessageHelper.Created(noti);
        }
    }
}