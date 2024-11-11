using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NCS.DSS.NotificationsListener.Services;
using NCS.DSS.NotificationsListener.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using JsonException = Newtonsoft.Json.JsonException;
using Microsoft.Azure.Cosmos;

namespace NCS.DSS.NotificationsListener.NotificationsListener.Function
{
    public class NotificationsListenerHttpTrigger
    {
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly ILogger<NotificationsListenerHttpTrigger> _logger;
        private readonly IDynamicHelper _dynamicHelper;
        private readonly ICosmosDBService _cosmosDBService;

        private static readonly string[] PropertyToExclude = { "TargetSite" };

        public NotificationsListenerHttpTrigger(
            IHttpRequestHelper httpRequestMessageHelper,
            ILogger<NotificationsListenerHttpTrigger> logger,
            IDynamicHelper dynamicHelper,
            ICosmosDBService cosmosDBService
        ) {
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _logger = logger;
            _dynamicHelper = dynamicHelper;
            _cosmosDBService = cosmosDBService;
        }

        [Function("Post")]
        [ProducesResponseType(typeof(Notification), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Notification Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Notification does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Post", Description = "Mock DSS Prime notification behaviour")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "URLEndpoint")] HttpRequest req)
        {
            _logger.LogInformation($"{typeof(NotificationsListenerHttpTrigger)} has been invoked");
            _logger.LogInformation("Attempting to deserialize the notification from the request");

            Notification notification;

            try
            {
                notification = await _httpRequestMessageHelper.GetResourceFromRequest<Notification>(req);

                if (notification == null)
                {
                    _logger.LogError("Notification object is null after deserialization.");
                    return new UnprocessableEntityResult();
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize the notification from the request body");
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertyToExclude));
            }

            string authHeader = string.Empty;

            if (req.Headers.TryGetValue("Authorization", out StringValues authToken))
            {
                authHeader = authToken.First();
                _logger.LogInformation("Authorization header found from request header");
            }
            else
            {
                _logger.LogWarning("Authorization header not found from the request header");
            }

            if (notification.ResourceURL != null)
            {
                _logger.LogInformation($"Processing ResourceURL: {notification.ResourceURL}");

                if (notification.ResourceURL.ToString().Contains("collections"))
                {
                    var lastIndexOf = notification.ResourceURL.ToString().LastIndexOf("/", StringComparison.Ordinal);
                    if (lastIndexOf != -1)
                    {
                        var collectionId = notification.ResourceURL.ToString().Substring(lastIndexOf + 1);

                        if (Guid.TryParse(collectionId, out var collectionGuid))
                        {
                            notification.CollectionId = collectionGuid;
                            _logger.LogInformation($"Extracted CollectionId: {collectionGuid}");
                        }
                        else
                        {
                            _logger.LogWarning("Failed to parse CollectionId from ResourceURL");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("ResourceURL does not contain a valid collection identifier");
                    }
                }
            }
            else
            {
                _logger.LogWarning("Notification ResourceURL property is null");
            }

            _logger.LogInformation($"Notification details; Customer ID: {notification.CustomerId} | URL: {notification.ResourceURL} " +
                $"| Last Modified Date: {notification.LastModifiedDate} | Touchpoint ID: {notification.TouchpointId} | Collection GUID: {notification.CollectionId}");
            _logger.LogInformation("Attempting to save notification to Cosmos DB");

            try
            {
                ItemResponse<Notification> response = await _cosmosDBService.CreateNewNotificationDocument(notification);

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    _logger.LogInformation("Notification was successfully written to Cosmos DB");

                    return new JsonResult(response.Resource, new JsonSerializerOptions())
                    {
                        StatusCode = (int)HttpStatusCode.OK // should this be 201 CREATED instead?
                    };
                }
                else
                {
                    _logger.LogError("Failure saving Notification to Cosmos DB");

                    return new JsonResult(new JsonSerializerOptions())
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failure saving Notification to Cosmos DB");
                
                return new JsonResult(new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
            finally
            {
                _logger.LogInformation($"{typeof(NotificationsListenerHttpTrigger)} has finished invocation");
            }
        }
    }
}