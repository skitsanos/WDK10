using System;

namespace WDK.API
{
	[Serializable]
	public class InvalidServerResponseException : Exception
	{
		public ServerResponse ServerResponse { get; private set; }

		public InvalidServerResponseException()
		{
		}

		public InvalidServerResponseException(string message, ServerResponse response)
			: base(message)
		{
			ServerResponse = response;
		}

		public InvalidServerResponseException(string message, ServerResponse response, Exception innerException)
			: base(message, innerException)
		{
			ServerResponse = response;
		}
	}
}