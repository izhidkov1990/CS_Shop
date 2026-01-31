using System;

namespace ItemService.Exceptions
{
    public class MarketplaceException : Exception
    {
        public int StatusCode { get; }

        public MarketplaceException(string message, int statusCode = 400) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
