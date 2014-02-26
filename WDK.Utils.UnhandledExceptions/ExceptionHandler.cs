using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using WDK.API.CouchDb;

/*
 * <httpModules>
 *  <add name="ExceptionHandler" type="WDK.Utils.UnhandledExceptions.ExceptionHandler, WDK.Utils.UnhandledExceptions" />
 * </httpModules>
*/

namespace WDK.Utils.UnhandledExceptions
{
    public class ExceptionHandler : IHttpModule
    {
        static int _unhandledExceptionCount = 0;

        public void Dispose()
        {
            //skip
        }

        public void Init(HttpApplication context)
        {
            context.Error += context_Error;
        }

        void context_Error(object sender, EventArgs e)
        {
            var doc = new ServerSideExceptionType
            {
                host = HttpContext.Current.Request.Url.Host,
                userAgent = HttpContext.Current.Request.UserAgent,
                userPlatform = HttpContext.Current.Request.Browser.Platform,
                queryString = HttpContext.Current.Request.ServerVariables["QUERY_STRING"],
                ip = Utils.getRemoteAddress()
            };

            var context = (HttpApplication)sender;
            if (context.Context.AllErrors == null) return;

            var listOfErrors = new List<ServerSideErrorDetailsType>();
            
            foreach (var err in context.Context.AllErrors)
            {
                listOfErrors.Add(new ServerSideErrorDetailsType()
                                {
                                    message = err.Message,
                                    stackTrace = err.StackTrace,
                                    source = err.Source
                                });
            }

            if (listOfErrors.Count <= 0) return;

            doc.errors = listOfErrors.ToArray();

            var db = new Database("localhost", 5984);
            db.createDocument(getAppSettings("ApplicationExceptions.db"), doc);
        }


        public static string getAppSettings(string key)
        {
            return System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath).AppSettings.Settings[key].Value;
        }

    }

    class ServerSideExceptionType
    {
        public string type = "ServerSideExceptionType";
        public DateTime createdOn = DateTime.Now;
        public string host;
        public string userAgent;
        public string userPlatform;
        public string ip;
        public string queryString;
        public ServerSideErrorDetailsType[] errors;
    }

    class ServerSideErrorDetailsType
    {
        public string source;
        public string message;
        public string stackTrace;
    }

    class Utils
    {
        public static string getRemoteAddress()
        {
            var ipAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            var remoteAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                var addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    remoteAddress = addresses[0];
                }
            }

            return remoteAddress;
        }
    }
}
