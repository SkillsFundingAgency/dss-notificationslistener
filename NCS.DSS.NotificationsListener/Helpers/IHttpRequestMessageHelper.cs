using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.NotificationsListener.Helpers
{
    public interface IHttpRequestMessageHelper
    {
        Task<T> GetMessageFromRequest<T>(HttpRequestMessage req);
    }
}