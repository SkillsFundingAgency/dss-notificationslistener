using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.NotificationsListener.Helpers;
using NCS.DSS.NotificationsListener.Models;
using NCS.DSS.NotificationsListener.NotificationsListener.Function;
using NCS.DSS.NotificationsListener.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Net;

namespace NCS.DSS.NotificationsListener.Tests
{
    [TestFixture]
    public class NotificationsListenerHttpTriggerTests
    {
        private Mock<IHttpRequestHelper> _httpRequestHelperMock = null!;
        private Mock<ILogger<NotificationsListenerHttpTrigger>> _loggerMock = null!;
        private Mock<IDynamicHelper> _dynamicHelperMock = null!;
        private Mock<ICosmosDBService> _cosmosDBServiceMock = null!;

        private HttpRequest _request = new DefaultHttpContext().Request;
        private NotificationsListenerHttpTrigger _function = null!;

        [SetUp]
        public void Setup()
        {
            _request = new DefaultHttpContext().Request;

            _httpRequestHelperMock = new Mock<IHttpRequestHelper>();
            _cosmosDBServiceMock = new Mock<ICosmosDBService>();
            _dynamicHelperMock = new Mock<IDynamicHelper>();
            _loggerMock = new Mock<ILogger<NotificationsListenerHttpTrigger>>();

            _function = new NotificationsListenerHttpTrigger(
                _httpRequestHelperMock.Object,
                _cosmosDBServiceMock.Object,
                _dynamicHelperMock.Object,
                _loggerMock.Object
            );
        }

        [Test]
        public async Task ReturnsOkResult_WhenNotificationIsProcessedSuccessfully()
        {
            // Arrange
            _request.Headers["Authorization"] = "Bearer token";

            var notification = new Notification
            {
                CustomerId = Guid.NewGuid(),
                ResourceURL = new Uri("https://example.com/collections/123"),
                LastModifiedDate = DateTime.UtcNow,
                TouchpointId = "00001"
            };

            var notificationItemResponse = new Mock<ItemResponse<Notification>>();
            notificationItemResponse.Setup(x => x.Resource).Returns(notification);
            notificationItemResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.Created);

            _httpRequestHelperMock.Setup(x => x.GetResourceFromRequest<Notification>(It.IsAny<HttpRequest>()))
                                  .ReturnsAsync(notification);

            _cosmosDBServiceMock.Setup(x => x.CreateNewNotificationDocument(It.IsAny<Notification>()))!
                                 .ReturnsAsync(notificationItemResponse.Object);

            // Act
            var result = await _function.Run(_request);
            JsonResult statusCodeResult = (JsonResult)result;

            // Assert
            Assert.That(statusCodeResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        [Test]
        public async Task ReturnsUnprocessableEntity_WhenNotificationIsNull()
        {
            // Arrange

            // Act
            var result = await _function.Run(_request);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityResult>());
        }

        [Test]
        public async Task ReturnsUnprocessableEntityObjectResult_WhenJsonExceptionOccurs()
        {
            // Arrange
            var jsonException = new JsonException("Error deserializing JSON.");

            _httpRequestHelperMock.Setup(x => x.GetResourceFromRequest<Notification>(It.IsAny<HttpRequest>()))
                                  .ThrowsAsync(jsonException);

            // Act
            var result = await _function.Run(_request);
            UnprocessableEntityObjectResult requestResponse = (UnprocessableEntityObjectResult)result;

            // Assert
            Assert.That(requestResponse.StatusCode, Is.EqualTo((int)HttpStatusCode.UnprocessableEntity));
        }

        [Test]
        public async Task ReturnsInternalServerError_WhenSaveNotificationFails()
        {
            // Arrange
            _request.Headers["Authorization"] = "Bearer token";

            var notification = new Notification
            {
                CustomerId = Guid.NewGuid(),
                ResourceURL = new Uri("https://example.com/collections/123"),
                LastModifiedDate = DateTime.UtcNow,
                TouchpointId = "00001"
            };

            _httpRequestHelperMock.Setup(x => x.GetResourceFromRequest<Notification>(It.IsAny<HttpRequest>()))
                                  .ReturnsAsync(notification);

            _cosmosDBServiceMock.Setup(x => x.CreateNewNotificationDocument(It.IsAny<Notification>()))
                                 .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _function.Run(_request);
            JsonResult statusCodeResult = (JsonResult)result;

            // Assert
            Assert.That(statusCodeResult.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
        }
    }
}
