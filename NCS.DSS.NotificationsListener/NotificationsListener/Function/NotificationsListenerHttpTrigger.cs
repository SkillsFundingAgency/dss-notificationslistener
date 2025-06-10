using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.NotificationsListener.Helpers;
using NCS.DSS.NotificationsListener.Models;
using NCS.DSS.NotificationsListener.Services;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace NCS.DSS.NotificationsListener.NotificationsListener.Function
{
    public class NotificationsListenerHttpTrigger
    {
        private readonly IHttpRequestHelper _httpRequestMessageHelper;
        private readonly ICosmosDBService _cosmosDBService;
        private readonly IDynamicHelper _dynamicHelper;
        private readonly ILogger<NotificationsListenerHttpTrigger> _logger;

        private static readonly string[] PropertyToExclude = { "TargetSite" };

        public NotificationsListenerHttpTrigger(
            IHttpRequestHelper httpRequestMessageHelper,
            ICosmosDBService cosmosDBService,
            IDynamicHelper dynamicHelper,
            ILogger<NotificationsListenerHttpTrigger> logger) 
        {
            _httpRequestMessageHelper = httpRequestMessageHelper;
            _cosmosDBService = cosmosDBService;
            _dynamicHelper = dynamicHelper;
            _logger = logger;
        }

        [Function("POST")]
        [ProducesResponseType(typeof(Notification), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Notification Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Notification does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "POST", Description = "Mock DSS Prime notification behaviour")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "URLEndpoint")] HttpRequest req)
        {
            _logger.LogInformation("{FunctionName} has been invoked", typeof(NotificationsListenerHttpTrigger));

            Notification notification;
            try
            {
                _logger.LogInformation("Attempting to deserialize the Notification from the request");
                notification = await _httpRequestMessageHelper.GetResourceFromRequest<Notification>(req);

                if (notification == null)
                {
                    _logger.LogError("Notification object is NULL after deserialization.");
                    return new UnprocessableEntityResult();
                }

                _logger.LogInformation("Successfully deserialized the Notification from the request");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize the Notification from the request body. Exception {ErrorMessage}", ex.Message);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertyToExclude));
            }

            if (req.Headers.TryGetValue("Authorization", out _))
            {
                _logger.LogInformation("Authorization header found from request header");
            }
            else
            {
                _logger.LogWarning("Authorization header not found from the request header");
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
                            _logger.LogInformation("Extracted CollectionId: {CollectionGuid}", collectionGuid);
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

            _logger.LogInformation("Notification details; Customer ID: {CustomerId}, URL: {ResourceURL}, Last Modified Date: {LastModifiedDate}, Touchpoint ID: {TouchpointId}, Collection GUID: {CollectionId}", 
                notification.CustomerId, 
                notification.ResourceURL, 
                notification.LastModifiedDate, 
                notification.TouchpointId, 
                notification.CollectionId);

            _logger.LogInformation("Attempting to save Notification to Cosmos DB");
            try
            {
                ItemResponse<Notification?> response = await _cosmosDBService.CreateNewNotificationDocument(notification);

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
                    _logger.LogError("Failed when saving Notification to Cosmos DB");

                    return new JsonResult(new JsonSerializerOptions())
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error has occurred when saving Notification to Cosmos DB. Exception {ErrorMessage}", ex.Message);
                
                return new JsonResult(new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
            finally
            {
                _logger.LogInformation("{FunctionName} has finished invocation", typeof(NotificationsListenerHttpTrigger));
            }
        }
    }
}