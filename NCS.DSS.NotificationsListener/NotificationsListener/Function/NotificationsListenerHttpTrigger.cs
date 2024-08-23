using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NCS.DSS.NotificationsListener.Annotations;
using NCS.DSS.NotificationsListener.Cosmos.Helper;
using NCS.DSS.NotificationsListener.Util;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Web.Http;
using JsonException = Newtonsoft.Json.JsonException;

namespace NCS.DSS.NotificationsListener.NotificationsListener.Function
{
    public class NotificationsListenerHttpTrigger
    {
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly ISaveNotificationToDatabase _saveNotification;
        private readonly ILogger _logger;
        private readonly IDynamicHelper _dynamicHelper;
        private static readonly string[] PropertyToExclude = { "TargetSite" };

        public NotificationsListenerHttpTrigger(
            IHttpRequestHelper httpRequestMessageHelper,
            ISaveNotificationToDatabase saveNotification,
            ILogger<NotificationsListenerHttpTrigger> logger,
            IDynamicHelper dynamicHelper)
        {
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _saveNotification = saveNotification;
            _logger = logger;
            _dynamicHelper = dynamicHelper;
        }

        [Function("Post")]
        [ProducesResponseType(typeof(Models.Notification), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Notification Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Notification does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new transfer resource.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "URLEndpoint")] HttpRequest req)
        {
            _logger.LogInformation("Processing a new notification request.");

            Models.Notification notification;

            try
            {
                _logger.LogInformation("Attempting to deserialize the notification from the request.");
                notification = await _httpRequestMessageHelper.GetResourceFromRequest<Models.Notification>(req);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize the notification.");
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertyToExclude));
            }

            if (notification == null)
            {
                _logger.LogWarning("Notification is null after deserialization.");
                return new UnprocessableEntityResult();
            }

            string authHeader = string.Empty;

            if (req.Headers.TryGetValue("Authorization", out StringValues authToken))
            {
                authHeader = authToken.First();
                _logger.LogInformation("Authorization header found.");
            }
            else
            {
                _logger.LogWarning("Authorization header is missing or invalid.");
            }

            if (notification.ResourceURL != null)
            {
                _logger.LogInformation("Processing ResourceURL: {ResourceURL}", notification.ResourceURL);
                if (notification.ResourceURL.ToString().Contains("collections"))
                {
                    var lastIndexOf = notification.ResourceURL.ToString().LastIndexOf("/", StringComparison.Ordinal);
                    if (lastIndexOf != -1)
                    {
                        var collectionId = notification.ResourceURL.ToString().Substring(lastIndexOf + 1);

                        if (Guid.TryParse(collectionId, out var collectionGuid))
                        {
                            notification.CollectionId = collectionGuid;
                            _logger.LogInformation("Extracted CollectionId: {CollectionId}", collectionGuid);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to parse CollectionId from ResourceURL.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("ResourceURL does not contain a valid collection identifier.");
                    }
                }
            }
            else
            {
                _logger.LogWarning("Notification ResourceURL is null.");
            }

            var noti = $"Customer Id : {notification.CustomerId}{Environment.NewLine}" +
                   $"URL : {notification.ResourceURL}{Environment.NewLine}" +
                   $"LastModifiedDate : {notification.LastModifiedDate}{Environment.NewLine}" +
                   $"Touchpoint Id : {notification.TouchpointId}{Environment.NewLine}" +
                   $"Collection Id : {notification.CollectionId}{Environment.NewLine}" +
                   $"Bearer : {authHeader}";

            _logger.LogInformation("Notification details: {NotificationDetails}", noti);

            try
            {
                _logger.LogInformation("Saving notification to the database.");
                await _saveNotification.SaveNotificationToDBAsync(notification);
                _logger.LogInformation("Notification saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving notification to the database.");
                return new InternalServerErrorResult();
            }

            return new JsonResult(noti, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}