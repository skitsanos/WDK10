using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Configuration;
using System.Web.Configuration;

namespace WDK.ContentManagement.Router
{
	public static class Utils
	{

		#region " GetWebConfigValue "
		public static object GetWebConfigValue(string Key)
		{
			var conf = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
			if (conf.AppSettings.Settings[Key] == null)
			{
				return null;
			}
			else
			{
				return conf.AppSettings.Settings[Key].Value;
			}
		}
		#endregion

		#region " GetApplicationName "
		public static string GetApplicationName()
		{
			Configuration conf = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
			MembershipSection mSection = new MembershipSection();
			mSection = (MembershipSection)conf.GetSection("system.web/membership");

			string appName = mSection.Providers[mSection.DefaultProvider].Parameters["applicationName"];
			if (string.IsNullOrEmpty(appName))
			{
				appName = System.Web.HttpContext.Current.Request.Url.Host;

				if (System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath != "/") appName += System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
			}

			return appName;
		}
		#endregion

		#region " GetEmbeddedContent "
		public static byte[] GetEmbeddedContent(string Resource)
		{
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
			string ns = "Wdk.IO";

			try
			{
				System.IO.Stream resStream = assembly.GetManifestResourceStream(ns + "." + Resource);
				if (resStream != null)
				{
					byte[] myBuffer = new byte[resStream.Length];
					resStream.Read(myBuffer, 0, (int)resStream.Length);
					return myBuffer;
				}
				else
				{
					return BytesOf("<!--- Resource {" + Resource.ToUpper() + "} not found under " + ns + " -->");

				}
			}
			catch (Exception ex)
			{
				return BytesOf(ex.ToString());
			}
		}
		#endregion

		#region " BytesOf "
		public static byte[] BytesOf(string Data)
		{
			return Encoding.UTF8.GetBytes(Data);
		}
		#endregion

		#region " GetODBCDriversList() "
		public static string[] GetODBCDriversList()
		{
			//HKEY_LOCAL_MACHINE\SOFTWARE\ODBC\ODBCINST.INI\ODBC Drivers
			Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE", false).OpenSubKey("ODBC", false).OpenSubKey("ODBCINST.INI", false).OpenSubKey("ODBC Drivers", false);

			return regKey.GetValueNames();
		}
		#endregion

		#region " FetchURL "
		public static string FetchURL(string URL)
		{
			if (string.IsNullOrEmpty(URL))
			{
				return "";
			}
			else
			{
				try
				{
					WebClient webClient = new WebClient();
					webClient.Headers.Add("pragma", "no-cache");
					webClient.Headers.Add("cache-control", "private");
					StreamReader streamReader = new StreamReader(webClient.OpenRead(URL));
					string str = streamReader.ReadToEnd();
					streamReader.Close();
					streamReader = null;
					webClient.Dispose();
					webClient = null;
					return str;
				}
				catch (Exception e)
				{
					return e.Message;
				}
			}
		}
		#endregion

		#region " UrlHome "
		public static string UrlHome()
		{
			System.Web.HttpRequest Request = System.Web.HttpContext.Current.Request;

			string url = "http://" + System.Web.HttpContext.Current.Request.Url.Host;
			//Request.ServerVariables("SERVER_NAME").ToLower
			if (System.Web.HttpContext.Current.Request.Url.Port != 80) url += ":" + System.Web.HttpContext.Current.Request.Url.Port;
			url += System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
			return url;
		}
		#endregion

		#region " Log "
		public static void Log(Exception ex)
		{
			//implement your logging here
		}
		#endregion

		public static void dump(string data)
		{
			var sw = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "log.txt", true);
			sw.WriteLine(data);
			sw.Close();
		}

	}
}
