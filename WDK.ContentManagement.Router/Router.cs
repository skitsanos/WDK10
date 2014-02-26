using System;
using System.Web.Routing;
using System.Web;

namespace WDK.ContentManagement.ContentRouter
{
	public class Route : IRouteHandler
	{
		public IHttpHandler GetHttpHandler(RequestContext context)
		{
			//var requestedPath = context.HttpContext.Server.MapPath(context.HttpContext.Request.Path);
			//if(System.IO.File.Exists(requestedPath))
			//{
			//    context.HttpContext.Response.Write(requestedPath);
			//    context.HttpContext.Response.End();
			//}

			if(context.RouteData.Values["plugin"] != null)
			{
				var plugin = context.RouteData.Values["plugin"] as string;
				plugin = char.ToUpper(plugin[0]) + plugin.Substring(1);

				var Response = context.HttpContext.Response;
				var Request = context.HttpContext.Request;

				Response.Clear();
				Response.ContentType = "text/html";

				//Response.Write("<b>OS:</b>"+System.Environment.OSVersion.VersionString + "<p/>");

				if(System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + "bin\\Route." + plugin + ".dll") == false && plugin.ToLower() != "jsonbridge")
				{
					Response.Write("There is an error occured during content rendering. Route." + plugin + ".dll is missing");
				}
				else
				{
					try
					{
						if(plugin.ToLower() == "jsonbridge")
						{
							//add to global.asax:
							//System.Web.Routing.RouteTable.Routes.Add(new System.Web.Routing.Route("{jsonbridge}/{classpath}/{method}", new WDK.ContentManagement.ContentRouter.Route()));

							var classpath = "";
							if(context.RouteData.Values["classpath"] != null)
							{
								classpath = context.RouteData.Values["classpath"] as string;
							}
							else
							{
								if(context.RouteData.Values["filename"] != null)
									classpath = context.RouteData.Values["filename"] as string;
							}

							var method = "";
							if(context.RouteData.Values["method"] != null)
								method = context.RouteData.Values["method"] as string;

							var bridgeHandler = new API.JsonBridge.HttpHandler
							{
							    classpath = classpath,
							    method = method
							};

							return bridgeHandler;
						}
						else
						{
							var mgr = Activator.CreateInstance(Type.GetType("Route." + plugin + ".HttpHandler, Route." + plugin, true, true));

							return (IHttpHandler)mgr;

							/*var filename = "*";

							if(context.RouteData.Values["filename"] != null)
								filename = context.RouteData.Values["filename"] as string;

							var fileContent = (byte[])pluginType.InvokeMember("Render", System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null, mgr, new object[] { filename });
							Response.Write(System.Text.Encoding.UTF8.GetString(fileContent));*/
						}

					}
					catch(Exception ex)
					{
						Response.Write("There is an error occured during content rendering. " + ex.Message);
					}
				}

				Response.End();
			}

			return null;
		}
	}
}
