using System.Net;
using System.Web;
using System.Xml.XPath;

namespace WDK.API.Yahoo
{
	public class YQL
	{
		private string query;
		private XPathDocument result;

		public YQL(string query)
		{
			SetQuery(query);
			result = _GetResult();
		}

		private XPathDocument _GetResult()
		{
			using (var responseStream = WebRequest.Create(query).GetResponse().GetResponseStream())
				return new XPathDocument(responseStream);
		}

		public XPathNavigator Execute()
		{
			return result.CreateNavigator();
		}

		private void SetQuery(string q)
		{
			query = "http://query.yahooapis.com/v1/public/yql?q=" + HttpUtility.UrlEncode(q) + "&format=xml&env=http%3A%2F%2Fdatatables.org%2Falltables.env";
		}
	}
}
