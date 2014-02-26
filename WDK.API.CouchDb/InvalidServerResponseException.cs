using System;

namespace WDK.API.CouchDb
{
    [Serializable]
    public class InvalidServerResponseException : Exception
    {
        public ServerResponse serverResponse { get; private set; }

        public InvalidServerResponseException()
        {
        }

        public InvalidServerResponseException(string message, ServerResponse response)
            : base(message)
        {
            serverResponse = response;
        }

        public InvalidServerResponseException(string message, ServerResponse response, Exception innerException)
            : base(message, innerException)
        {
            serverResponse = response;
        }
    }
}