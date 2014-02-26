using System;
using System.Web;
using System.Web.SessionState;
using Antlr3.ST;

namespace Route.Content
{
	public class HttpHandler : IHttpHandler, IRequiresSessionState
	{
		#region Implementation of IHttpHandler

		public void ProcessRequest(HttpContext context)
		{
			var Response = context.Response;
			var Request = context.Request;

			Response.ContentType = "text/html";

			var filename = System.IO.Path.GetFileNameWithoutExtension(Request.Path);

			var pages = new WDK.ContentManagement.Pages.Manager();
			var foundPage = pages.getPageByFilename(filename);

			if(foundPage.title.ToLower().Contains("not found"))
			{
				Response.Write(foundPage.content);
			}
			else
			{
				var pathToMasterPage = AppDomain.CurrentDomain.BaseDirectory + foundPage.masterPage;
				if(System.IO.File.Exists(pathToMasterPage))
				{
					var sr = new System.IO.StreamReader(pathToMasterPage);
					var template = sr.ReadToEnd();
					sr.Close();

					var q = new StringTemplate(template);
					q.SetAttribute("show_search_results", true);
					q.SetAttribute("year", DateTime.Now.Year);
					q.SetAttribute("title", foundPage.title);
					q.SetAttribute("content", foundPage.content);

					Response.Write(q.ToString());
				}
				else
				{
					Response.Write(foundPage.content);
				}
			}
		}

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}

		#endregion
	}
}
