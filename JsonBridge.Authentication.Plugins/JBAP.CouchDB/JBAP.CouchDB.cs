//
// JBAP.couchDB.cs
// CouchDB JsonBridge Authentication plugin
//
// Author:
//       Skitsanos <info@skitsanos.com>
//
// Copyright (c) 2012 Skitsanos Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;

namespace JBAP
{
	public class CouchDB
	{
		public bool validate(string authHeaderValue)
		{
			log(authHeaderValue);

			if (!authHeaderValue.Contains(":"))
				return false;

			var authPair = authHeaderValue.Split(':');

			var db = new WDK.API.CouchDb("localhost", 5984);
			var result = bool.Parse(db.getDesignListAsJson("pmware", "Users", "validate/viewAll?u=" + authPair[0] + "&p=" + authPair[1]));
			
			log(result.ToString());

			return result;
		}

		private static void log(string data)
		{
			var db = new WDK.API.CouchDb("localhost", 5984);
			db.createDocument("pmware", new ApplicationLogType()
			                            	{
			                            		notes = data
			                            	});
		}
	}

	class ApplicationLogType
	{
		public string type = "ApplicationLogType";
		public DateTime createdOn = DateTime.Now;
		public string createdBy = "$JBAP.CouchDB$";
		public string notes;
	}
}
