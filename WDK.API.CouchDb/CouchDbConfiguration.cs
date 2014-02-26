using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace WDK.API.CouchDb
{
    public class CouchDbConfiguration
    {
    }

    public class CouchDbEndpoint
    {
        public string db = "";

        public string host = "localhost";
        public int port = 5984;

        public string username;
        public string password;

        public int timeout = System.Threading.Timeout.Infinite;

        public bool useSsl = false;

        #region " ExtractValue "
        private string extractValue(string key, string sourceString, char separator)
        {
            var result = "";

            var arr = new List<string>(sourceString.Split(separator));
            if (sourceString.Contains(key + "="))
            {
                foreach (var keyValuePair in arr.Where(keyValuePair => keyValuePair.Split('=')[0] == key))
                {
                    result = keyValuePair.Split('=')[1];
                }
            }

            return result;
        }
        #endregion

        public CouchDbEndpoint()
        {
        }
        public CouchDbEndpoint(string ConnectionString)
        {
            host = extractValue("host", ConnectionString, ';');
            port = int.Parse(extractValue("port", ConnectionString, ';'));

            db = extractValue("db", ConnectionString, ';');

            username = extractValue("username", ConnectionString, ';');
            password = extractValue("password", ConnectionString, ';');

            useSsl = !String.IsNullOrEmpty(extractValue("useSsl", ConnectionString, ';')) && bool.Parse(extractValue("useSsl", ConnectionString, ';'));
        }

        #region " getUrl "
        /// <summary>
        /// Builds CouchDB API URL from Endpoint parameters provided
        /// </summary>
        /// <returns>string</returns>
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

        public HttpWebRequest getRequest(string url, string method)
        {
            var request = WebRequest.Create(getUrl() + url) as HttpWebRequest;

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

                request.ContentType = "application/json";

                return request;
            }
            return null;
        }

        public HttpWebRequest getRequest(string url, string method, string postData)
        {
            var request = WebRequest.Create(getUrl() + url) as HttpWebRequest;

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

                request.ContentType = "application/json";

                if (postData != null)
                {
                    var bytes = Encoding.UTF8.GetBytes(postData);

                    request.ContentLength = bytes.Length;

                    using (var ps = request.GetRequestStream())
                    {
                        ps.Write(bytes, 0, bytes.Length);
                    }
                }

                return request;
            }
            return null;
        }

        public ServerResponse getResponse(HttpWebRequest request, bool isBinaryResult)
        {
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

                    result.contentString = null;
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
            }

            return result;
        }
    }
}
