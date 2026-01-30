using System.Net;

namespace ItemService.Exceptions
{
    public class SteamApiException: Exception
    {
        public HttpStatusCode StatusCode { get; }

        public SteamApiException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
