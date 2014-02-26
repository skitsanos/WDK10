namespace WDK.API.CouchDb
{
	public class ServerResponse
	{
		public bool isBinaryResult;

		//public WebHeaderCollection Headers;

		//public HttpStatusCode StatusCode;
		//public string StatusDescription;

		public string contentType;
		//public long ContentLength;

		public string contentString;
		public byte[] contentBytes;
	}
}