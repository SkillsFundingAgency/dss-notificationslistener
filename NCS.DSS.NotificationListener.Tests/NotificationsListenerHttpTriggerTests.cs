using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.NotificationsListener.Cosmos.Helper;
using NCS.DSS.NotificationsListener.Models;
using NCS.DSS.NotificationsListener.NotificationsListener.Function;
using NCS.DSS.NotificationsListener.Util;
using Newtonsoft.Json;
using System.Net;
using NUnit.Framework;

namespace NCS.DSS.NotificationsListener.Tests
{
    [TestFixture]
    public class NotificationsListenerHttpTriggerTests
    {
        private Mock<IHttpRequestHelper> _httpRequestHelperMock;
        private Mock<ISaveNotificationToDatabase> _saveNotificationMock;
        private Mock<ILogger<NotificationsListenerHttpTrigger>> _loggerMock;
        private Mock<IDynamicHelper> _dynamicHelperMock;

        private HttpRequest _request;
        private NotificationsListenerHttpTrigger _function;

        [SetUp]
        public void Setup()
        {
            _request = new DefaultHttpContext().Request;

            _httpRequestHelperMock = new Mock<IHttpRequestHelper>();
            _saveNotificationMock = new Mock<ISaveNotificationToDatabase>();
            _loggerMock = new Mock<ILogger<NotificationsListenerHttpTrigger>>();
            _dynamicHelperMock = new Mock<IDynamicHelper>();
            _function = new NotificationsListenerHttpTrigger(
                _httpRequestHelperMock.Object, 
                _saveNotificationMock.Object, 
                _loggerMock.Object, 
                _dynamicHelperMock.Object);
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

            _httpRequestHelperMock.Setup(x => x.GetResourceFromRequest<Notification>(It.IsAny<HttpRequest>()))
                                  .ReturnsAsync(notification);

            // Act
            var result = await _function.Run(_request);

            // Assert
            var jsonResult = result as JsonResult;
            Assert.That(jsonResult, Is.Not.Null);
            Assert.That(jsonResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        [Test]
        public async Task ReturnsUnprocessableEntity_WhenNotificationIsNull()
        {
            // Arrange
            _httpRequestHelperMock.Setup(x => x.GetResourceFromRequest<Notification>(It.IsAny<HttpRequest>()))
                                  .ReturnsAsync((Notification)null);

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

            // Assert
            var unprocessableEntityObjectResult = result as UnprocessableEntityObjectResult;
            Assert.That(unprocessableEntityObjectResult, Is.Not.Null);
            Assert.That(unprocessableEntityObjectResult.StatusCode,
                Is.EqualTo((int)HttpStatusCode.UnprocessableEntity));
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

            _saveNotificationMock.Setup(x => x.SaveNotificationToDBAsync(It.IsAny<Notification>()))
                                 .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _function.Run(_request);

            // Assert
            var statusCodeResult = result as StatusCodeResult;
            Assert.That(statusCodeResult, Is.Not.Null);
            Assert.That(statusCodeResult.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
        }
    }
}
