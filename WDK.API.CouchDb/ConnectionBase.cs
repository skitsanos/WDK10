using System;
using System.Net.Mime;
using System.Text;
using System.Net;
using System.IO;

namespace WDK.API.CouchDb
{
    public class ConnectionBase
    {
        #region " Variables and Properties "

        private string _host = "localhost";

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

        private int _port = 5984;

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

        public string getUrl()
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

            if (useSsl)
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
        protected ServerResponse doRequest(string url, string method, bool isBinaryResult)
        {
            return doRequest(url, method, null, null, isBinaryResult);
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
        protected ServerResponse doRequest(string url, string method, string postdata, string contenttype, bool isBinaryResult)
        {
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

                    request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password)));
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

	        if (response == null) return result;
	        result.contentType = response.ContentType;

	        if (isBinaryResult)
	        {
		        result.isBinaryResult = true;

		        var buffer = new byte[32768];
		        using (var ms = new MemoryStream())
		        {
			        while (true)
			        {
				        var responseStream = response.GetResponseStream();
				        if (responseStream == null) continue;
				        var read = responseStream.Read(buffer, 0, buffer.Length);

				        if (read <= 0)
				        {
					        result.contentBytes = ms.ToArray();

					        break;
				        }

				        ms.Write(buffer, 0, read);
			        }
		        }

		        result.contentString = Convert.ToBase64String(result.contentBytes);
	        }
	        else
	        {
		        result.isBinaryResult = false;
		        result.contentBytes = null;

		        var encode = Encoding.GetEncoding("utf-8");

		        using (var reader = new StreamReader(response.GetResponseStream(), encode))
		        {
			        result.contentString = reader.ReadToEnd();
		        }
	        }

	        return result;
        }

        protected ServerResponse doRequest(string url, string method, WebHeaderCollection headers, string postdata, string contenttype, bool isBinaryResult)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;

            if (request != null)
            {
                request.Method = method;
                // Set an infinite timeout on this for now, because executing a temporary view (for example) can take a very long time
                request.Timeout = System.Threading.Timeout.Infinite;

                request.Headers = headers;

                // Set authorization header
                if ((request.Headers != null) && (request.Headers[HttpRequestHeader.Authorization] != null))
                {
                    if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
                    {
                        if (request.Headers == null)
                        {
                            request.Headers = new WebHeaderCollection();
                        }

                        request.Headers.Add(HttpRequestHeader.Authorization, "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password)));
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

	        if (response == null) return result;
	        result.contentType = response.ContentType;

	        if (isBinaryResult)
	        {
		        var buffer = new byte[32768];
		        using (var ms = new MemoryStream())
		        {
			        while (true)
			        {
				        var responseStream = response.GetResponseStream();
				        if (responseStream == null) continue;
				        var read = responseStream.Read(buffer, 0, buffer.Length);

				        if (read <= 0)
				        {
					        result.contentBytes = ms.ToArray();

					        break;
				        }

				        ms.Write(buffer, 0, read);
			        }
		        }

		        result.contentString = Convert.ToBase64String(result.contentBytes);
	        }
	        else
	        {
		        result.isBinaryResult = false;
		        result.contentBytes = null;

		        using (var reader = new StreamReader(response.GetResponseStream()))
		        {
			        result.contentString = reader.ReadToEnd();
		        }
	        }

	        return result;
        }

        #endregion

        #region " SanitizeOutput "

        protected string sanitizeInOutput(string content)
        {
            var result = content;

            result = result.Replace(@"\\r\\n", "THIS_IS_NOT_A_RETURN_NEWLINE");


            result = result.Replace(@"\\n", "THIS_IS_NOT_A_NEWLINE");


            result = result.Replace(@"\\t", "THIS_IS_NOT_A_TAB");

            return result;
        }

        protected string sanitizeOutOutput(string content)
        {
            var result = content;
            result = result.Trim('"');

            result = result.Replace(@"\r\n", Environment.NewLine);
            result = result.Replace(@"\n", Environment.NewLine);
            result = result.Replace(@"\t", "\t");


            result = result.Replace("THIS_IS_NOT_A_NEWLINE", @"\n");

            result = result.Replace("THIS_IS_NOT_A_RETURN_NEWLINE", @"\r\n");

            result = result.Replace("THIS_IS_NOT_A_TAB", @"\t");

            return result;
        }

        #endregion
    }
}
