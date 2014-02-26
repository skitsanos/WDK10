/*
 * WDK.ContentManagement.ContentRouter.RouterResponse
 * Content Router Response Object
 * Copyright © 2010, Skitsanos
 * @author skitsanos (info@skitsanos.com)
 * @version 1.0
*/

namespace WDK.ContentManagement.ContentRouter
{
	public class RouterResponse
	{
		public string status = "200 OK";
		public string contentType = "text/html";
		public byte[] content;
	}
}