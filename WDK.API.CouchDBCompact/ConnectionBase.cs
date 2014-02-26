using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Net;
using System.IO;

namespace WDK.API
{
	public class ConnectionBase
	{
		#region " Variables and Properties "

		protected string _host = "localhost";
		public string host
		{
			get
			{
				return _host;
			}
			set
			{
				_host = value;
			}
		}

		protected int _port = 5984;
		public int port
		{
			get
			{
				return _port;
			}
			set
			{
				_port = value;
			}
		}

		public string username
		{
			get;
			set;
		}
		public string password
		{
			get;
			set;
		}

		public bool useSsl
		{
			get;
			set;
		}

		#endregion

		#region " GetUrl "

		protected string getUrl()
		{
			var url = "http://";

			if (useSsl)
			{
				url = "https://";
			}

			if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
			{
				url += username + ":" + password + "@";
			}

			if (host.Contains("cloudant.com") && useSsl)
			{
				url += host;
			}
			else
			{
				url += host + ":" + port;
			}

			return url;
		}

		#endregion

		#region " DoRequest "

		/// <summary>
		/// Internal helper to make an HTTP request and return the response.
		/// Throws an exception in the event of any kind of failure.
		/// Overloaded - use the other version if you need to post data with the request.
		/// </summary>
		/// <param name="url">The URL</param>
		/// <param name="method">The method, e.g. "GET"</param>
		/// <param name="isBinaryResult"> </param>
		/// <returns>The server's response</returns>
		protected ServerResponse DoRequest(string url, string method, bool isBinaryResult)
		{
			return DoRequest(url, method, null, null, isBinaryResult);
		}

		/// <summary>
		/// Internal helper to make an HTTP request and return the response.
		/// Throws an exception in the event of any kind of failure.
		/// Overloaded - use the other version if no post data is required.
		/// </summary>
		/// <param name="url">The URL</param>
		/// <param name="method">The method, e.g. "GET"</param>
		/// <param name="postdata">Data to be posted with the request, or null if not required.</param>
		/// <param name="contenttype">The content type to send, or null if not required.</param>
		/// <param name="isBinaryResult"> </param>
		/// <returns>The server's response</returns>
		protected ServerResponse DoRequest(string url, string method, string postdata, string contenttype, bool isBinaryResult)
		{
            ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();

			var request = WebRequest.Create(url) as HttpWebRequest;

			if (request != null)
			{
				request.Method = method;
				// Set an infinite timeout on this for now, because executing a temporary view (for example) can take a very long time
				request.Timeout = System.Threading.Timeout.Infinite;

				// Set authorization header
				if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
				{
					if (request.Headers == null)
					{
						request.Headers = new WebHeaderCollection();
					}
					request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password)));
				}

				if (contenttype != null)
				{
					request.ContentType = contenttype;
				}

				if (postdata != null)
				{
					var bytes = Encoding.UTF8.GetBytes(postdata);

					request.ContentLength = bytes.Length;

					using (var ps = request.GetRequestStream())
					{
						ps.Write(bytes, 0, bytes.Length);
					}
				}
			}
			HttpWebResponse response = null;
            
			try
			{
				if (request != null)
				{
					response = request.GetResponse() as HttpWebResponse;
				}
			}
			catch (WebException ex)
			{
				response = ex.Response as HttpWebResponse;
			}

			var result = new ServerResponse();

			if (response != null)
			{
				result.ContentType = response.ContentType;

				if (isBinaryResult)
				{
					result.isBinaryResult = true;

					var buffer = new byte[32768];
					using (var ms = new MemoryStream())
					{
						while (true)
						{
							var read = response.GetResponseStream().Read(buffer, 0, buffer.Length);

							if (read <= 0)
							{
								result.ContentBytes = ms.ToArray();

								break;
							}

							ms.Write(buffer, 0, read);
						}
					}

					result.ContentString = null;
				}
				else
				{
					result.isBinaryResult = false;
					result.ContentBytes = null;

					using (var reader = new StreamReader(response.GetResponseStream()))
					{
						result.ContentString = reader.ReadToEnd();
					}
				}
			}

			return result;
		}

		protected ServerResponse DoRequest(string url, string method, WebHeaderCollection headers, string postdata, string contenttype, bool isBinaryResult)
		{
			var request = WebRequest.Create(url) as HttpWebRequest;

			if (request != null)
			{
				request.Method = method;
				// Set an infinite timeout on this for now, because executing a temporary view (for example) can take a very long time
				request.Timeout = System.Threading.Timeout.Infinite;

				request.Headers = headers;

				// Set authorization header
				if ((request.Headers != null) && (request.Headers["Authorization"] != null))
				{
					if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
					{
						if (request.Headers == null)
						{
							request.Headers = new WebHeaderCollection();
						}

						request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password)));
					}
				}

				if (contenttype != null)
				{
					request.ContentType = contenttype;
				}

				if (postdata != null)
				{
					var bytes = Encoding.UTF8.GetBytes(postdata);

					request.ContentLength = bytes.Length;

					using (var ps = request.GetRequestStream())
					{
						ps.Write(bytes, 0, bytes.Length);
					}
				}
			}

			HttpWebResponse response = null;

			try
			{
				if (request != null)
				{
					response = request.GetResponse() as HttpWebResponse;
				}
			}
			catch (WebException ex)
			{
				response = ex.Response as HttpWebResponse;
			}

			var result = new ServerResponse();

			if (response != null)
			{
				result.ContentType = response.ContentType;

				if (isBinaryResult)
				{
					var buffer = new byte[32768];
					using (var ms = new MemoryStream())
					{
						while (true)
						{
							var read = response.GetResponseStream().Read(buffer, 0, buffer.Length);

							if (read <= 0)
							{
								result.ContentBytes = ms.ToArray();

								break;
							}

							ms.Write(buffer, 0, read);
						}
					}

					result.ContentString = null;
				}
				else
				{
					result.isBinaryResult = false;
					result.ContentBytes = null;

					using (var reader = new StreamReader(response.GetResponseStream()))
					{
						result.ContentString = reader.ReadToEnd();
					}
				}
			}

			return result;
		}

		#endregion

		#region " SanitizeOutput "

		protected string sanitizeOutput(string content)
		{
			var result = content;

			result = result.Trim('"');

			result = result.Replace(@"\\r\\n", "THIS_IS_NOT_A_RETURN_NEWLINE");
			result = result.Replace(@"\r\n", Environment.NewLine);

			result = result.Replace(@"\\n", "THIS_IS_NOT_A_NEWLINE");
			result = result.Replace(@"\n", Environment.NewLine);
			result = result.Replace("THIS_IS_NOT_A_NEWLINE", @"\n");

			result = result.Replace("THIS_IS_NOT_A_RETURN_NEWLINE", @"\r\n");

			result = result.Replace(@"\\t", "THIS_IS_NOT_A_TAB");
			result = result.Replace(@"\t", "\t");
			result = result.Replace("THIS_IS_NOT_A_TAB", @"\t");

			result = result.Replace("\\\"", "\"");
			result = result.Replace("\\\\", "\\");

			return result;
		}

		#endregion
	}
    public class TrustAllCertificatePolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint sp, X509Certificate cert, WebRequest req, int problem)
        {
            return true;
        }
    }
}