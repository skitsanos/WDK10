using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Security.Cryptography;

namespace WDK.API.CouchDb
{
	public class UserManagement : ConnectionBase
	{
		#region " Constructor "

		public UserManagement(string host, int port)
		{
			if (host.StartsWith("http://"))
			{
				host = host.Replace("http://", "");
			}

			if (host.StartsWith("https://"))
			{
				host = host.Replace("https://", "");

				useSsl = true;
			}

			if (host.Contains("cloudant.com"))
			{
				useSsl = true;
			}

			this.host = host;
			this.port = port;

			username = String.Empty;
			password = String.Empty;
		}

		public UserManagement(string host, int port, string username, string password)
		{
			if (host.StartsWith("http://"))
			{
				host = host.Replace("http://", "");
			}

			if (host.StartsWith("https://"))
			{
				host = host.Replace("https://", "");

				useSsl = true;
			}

			if (host.Contains("cloudant.com"))
			{
				useSsl = true;
			}

			this.host = host;
			this.port = port;

			this.username = username;
			this.password = password;
		}

		#endregion

		#region " isExists "

		public bool isExists(string userId)
		{
			bool ret = false;

			try
			{
				ServerResponse result = doRequest(getUrl() + "/_users/org.couchdb.user:" + userId, "GET", false);

				if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
				{
					var jo = (JObject)JsonConvert.DeserializeObject(result.contentString);

					if (jo["error"] == null)
					{
						ret = true;
					}
				}
				else
				{
					throw new InvalidServerResponseException("Invalid Server Response!", result);
				}
			}
			catch (InvalidServerResponseException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}

			return ret;
		}

		#endregion

		#region " get "

		public string get(string userId)
		{
			string ret;

			try
			{
				ServerResponse result = doRequest(getUrl() + "/_users/org.couchdb.user:" + userId, "GET", false);

				if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
				{
					var jo = (JObject)JsonConvert.DeserializeObject(result.contentString);

					if (jo["error"] != null)
					{
						throw new Exception("The username does not exist or you do not have permissions to view it!");
					}

					ret = sanitizeOutOutput(result.contentString);
				}
				else
				{
					throw new InvalidServerResponseException("Invalid Server Response!", result);
				}
			}
			catch (InvalidServerResponseException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}

			return ret;
		}

		#endregion

		#region " add "

		public bool add(string userId, string userPassword, List<string> userRoles)
		{
			bool ret = false;

			try
			{
				if (isExists(userId))
				{
					throw new Exception("The username already exists!");
				}

				var content = new JObject();

				content["_id"] = "org.couchdb.user:" + userId;
				content["type"] = "user";
				content["name"] = userId;

				// Generate salt
				var saltBytes = new byte[16];
				(new RNGCryptoServiceProvider()).GetNonZeroBytes(saltBytes);
				content["salt"] = BitConverter.ToString(saltBytes).Replace("-", "").ToLower();

				// Generate password hash
				var buffer = (new UTF8Encoding()).GetBytes(userPassword + content["salt"]);
				content["password_sha"] = BitConverter.ToString((new SHA1CryptoServiceProvider()).ComputeHash(buffer)).Replace("-", "").ToLower();

				// Roles
				content["roles"] = new JArray();
				if (userRoles.Count != 0)
				{
					foreach (var item in userRoles)
					{
						(content["roles"] as JArray).Add(item);
					}
				}

				var converters = new JsonConverter[] { new IsoDateTimeConverter() };

				ServerResponse result = doRequest(getUrl() + "/_users/org.couchdb.user:" + userId, "PUT", JsonConvert.SerializeObject(content, converters), "application/json", false);

				if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
				{
					var jo = (JObject)JsonConvert.DeserializeObject(result.contentString);

					if (jo["ok"] != null)
					{
						ret = true;
					}
					else if (jo["error"] != null)
					{
						throw new Exception(jo["reason"].ToString());
					}
				}
				else
				{
					throw new InvalidServerResponseException("Invalid Server Response!", result);
				}
			}
			catch (InvalidServerResponseException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}

			return ret;
		}

		#endregion

		#region " update "

		public bool update(string userId, string userPassword, List<string> userRoles)
		{
			bool ret = false;

			try
			{
				if (!isExists(userId))
				{
					throw new Exception("The username does not exist or you do not have permissions to edit it!");
				}

				// Get existing user
				ServerResponse result = doRequest(getUrl() + "/_users/org.couchdb.user:" + userId, "GET", false);

				if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
				{
					var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

					// Generate salt
					var saltBytes = new byte[16];
					(new RNGCryptoServiceProvider()).GetNonZeroBytes(saltBytes);
					ro["salt"] = BitConverter.ToString(saltBytes).Replace("-", "").ToLower();

					// Generate password hash
					var buffer = (new UTF8Encoding()).GetBytes(userPassword + ro["salt"]);
					ro["password_sha"] = BitConverter.ToString((new SHA1CryptoServiceProvider()).ComputeHash(buffer)).Replace("-", "").ToLower();

					// Roles
					ro["roles"] = new JArray();
					if (userRoles.Count != 0)
					{
						foreach (var item in userRoles)
						{
							(ro["roles"] as JArray).Add(item);
						}
					}

					var converters = new JsonConverter[] { new IsoDateTimeConverter() };

					result = doRequest(getUrl() + "/_users/org.couchdb.user:" + userId, "PUT", JsonConvert.SerializeObject(ro, converters), "application/json", false);

					if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
					{
						var jo = (JObject)JsonConvert.DeserializeObject(result.contentString);

						if (jo["ok"] != null)
						{
							ret = true;
						}
						else if (jo["error"] != null)
						{
							throw new Exception(jo["reason"].ToString());
						}
					}
					else
					{
						throw new InvalidServerResponseException("Invalid Server Response!", result);
					}
				}
			}
			catch (InvalidServerResponseException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}

			return ret;
		}

		#endregion

		#region " delete "

		public bool delete(string userId)
		{
			bool ret = false;

			try
			{
				if (!isExists(userId))
				{
					throw new Exception("The username does not exist or you do not have permissions to delete it!");
				}

				ServerResponse result = doRequest(getUrl() + "/_users/org.couchdb.user:" + userId, "GET", false);

				var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

				result = doRequest(getUrl() + "/_users/org.couchdb.user:" + userId + "?rev=" + (string)ro["_rev"], "DELETE", false);

				if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
				{
					var jo = (JObject)JsonConvert.DeserializeObject(result.contentString);

					if (jo["ok"] != null)
					{
						ret = true;
					}
					else if (jo["error"] != null)
					{
						throw new Exception(jo["reason"].ToString());
					}
				}
				else
				{
					throw new InvalidServerResponseException("Invalid Server Response!", result);
				}
			}
			catch (InvalidServerResponseException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message);
			}

			return ret;
		}

		#endregion
	}
}