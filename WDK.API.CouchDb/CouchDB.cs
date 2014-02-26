using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

/*
** (notes)
** http://devzone.zend.com/article/12523-Getting-started-with-CouchDB-meet-PHP-on-Couch
** http://guide.couchdb.org/draft/design.html
** http://www.cmlenz.net/archives/2007/10/couchdb-joins
** http://github.com/lenalena/couchdb/blob/trunk/share/www/script/jquery.couch.js
*/

namespace WDK.API.CouchDb
{
    /// <summary>
    /// A simple wrapper class for the CouchDB HTTP API. No
    /// initialisation is necessary, just create an instance and
    /// call the appropriate methods to interact with CouchDB.
    /// All methods throw exceptions when things go wrong.
    /// </summary>
    public class Database : ConnectionBase
    {
        #region " Constructor "

        public Database()
        {
        }

        public Database(CouchDbEndpoint endpoint)
        {
            host = endpoint.host;
            port = endpoint.port;
            useSsl = endpoint.useSsl;
            username = endpoint.username;
            password = endpoint.password;
        }

        public Database(string host, int port)
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

        public Database(string host, int port, bool useSsl)
        {
            this.useSsl = useSsl;
            this.host = host;
            this.port = port;

            username = String.Empty;
            password = String.Empty;
        }

        public Database(string host, int port, string username, string password)
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

        public Database(string host, int port, bool useSsl, string username, string password)
        {
            this.useSsl = useSsl;

            this.host = host;
            this.port = port;

            this.username = username;
            this.password = password;
        }

        #endregion

        #region " Server Functions "

        #region " Server Exists "
        /// <summary>
        /// Check if the Server exist
        /// </summary>
        public bool serverExists()
        {
            var result = doRequest(getUrl() + "/", "GET", false);

            if (string.IsNullOrEmpty(result.contentString) && result.contentBytes == null)
                return false;

            if (!result.contentType.Contains("text/plain") && !result.contentType.Contains("application/json"))
            {
                throw new InvalidServerResponseException("Invalid Server Response!", result);
            }
            return true;
        }

        #endregion

        #region " Version "

        /// <summary>
        /// Gets CouchDB Server version
        /// </summary>
        /// <returns></returns>
        public string version()
        {
            var result = doRequest(getUrl() + "/", "GET", false);

            if (!result.contentType.Contains("text/plain") && !result.contentType.Contains("application/json"))
            {
                throw new InvalidServerResponseException("Invalid Server Response!", result);
            }

            var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

            return (string)ro["version"];
        }

        #endregion

        #region " Welcome String "

        /// <summary>
        /// Gets CouchDB Server Welcome string
        /// </summary>
        /// <returns></returns>
        public string welcomeString()
        {
            var result = doRequest(getUrl() + "/", "GET", false);

            if (!result.contentType.Contains("text/plain") && !result.contentType.Contains("application/json"))
            {
                throw new InvalidServerResponseException("Invalid Server Response!", result);
            }

            var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

            return (string)ro["couchdb"];
        }

        #endregion

        #region " GetActiveTasks "

        public List<JObject> getActiveTasks()
        {
            try
            {
                var result = doRequest(getUrl() + "/_active_tasks", "GET", false);

                if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
                {
                    var ro = (JArray)JsonConvert.DeserializeObject(result.contentString);

                    return (from JObject row in ro select row).ToList();
                }

                throw new InvalidServerResponseException("Invalid Server Response!", result);
            }
            catch (InvalidServerResponseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #endregion

        #region " Database Functions "

        #region " GetDatabases "

        /// <summary>
        /// Get a list of database on the server.
        /// </summary>
        /// <returns>A string array containing the database names
        /// </returns>
        public List<string> getDatabases()
        {
            try
            {
                var result = doRequest(getUrl() + "/_all_dbs", "GET", false);

                if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
                {
                    var ro = (JArray)JsonConvert.DeserializeObject(result.contentString);

                    var ret = new List<string>();

                    JsonConvert.PopulateObject(ro.ToString(), ret);

                    return ret;
                }

                throw new InvalidServerResponseException("Invalid Server Response!", result);
            }
            catch (InvalidServerResponseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " DatabaseExists "

        /// <summary>
        /// Checks if database exists
        /// </summary>
        /// <param name="db">The database name</param>
        /// <returns></returns>
        public bool databaseExists(string db)
        {
            var ret = false;

            var result = doRequest(getUrl() + "/" + db, "GET", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

                if (ro["error"] == null)
                {
                    ret = true;
                }

                return ret;
            }

            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        #endregion

        #region " CreateDatabase "

        /// <summary>
        /// Create a new database.
        /// </summary>
        /// <param name="db">The database name</param>
        /// <returns>(bool) True if database created</returns>
        public bool createDatabase(string db)
        {
            var ret = false;

            try
            {
                var result = doRequest(getUrl() + "/" + db.ToLower(), "PUT", false);

                if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
                {
                    if (result.contentString == "{\"ok\":true}\n")
                    {
                        ret = true;
                    }
                    else if (result.contentString.Contains("\"error\""))
                    {
                        var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

                        throw new Exception(ro["reason"].ToString());
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

        #region " DeleteDatabase "

        /// <summary>
        /// Delete a database
        /// </summary>
        /// <param name="db">The name of the database to delete</param>
        public bool deleteDatabase(string db)
        {
            var ret = false;

            var result = doRequest(getUrl() + "/" + db, "DELETE", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                if (result.contentString == "{\"ok\":true}\n")
                {
                    ret = true;
                }
            }
            else
            {
                throw new InvalidServerResponseException("Invalid Server Response!", result);
            }

            return ret;
        }

        #endregion



        #endregion

        #region " Document Functions "

        #region " GetAllDocuments "

        /// <summary>
        /// Get information on all the documents in the given database.
        /// </summary>
        /// <param name="db">The database name</param>
        /// <returns>An array of DocInfo instances</returns>	
        public List<DocumentInfo> getAllDocuments(string db)
        {
            var result = doRequest(getUrl() + "/" + db + "/_all_docs", "GET", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                var serializer = new JsonSerializerSettings();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());

                var ro = (JObject)JsonConvert.DeserializeObject(result.contentString, serializer);

                return (from JObject row in ro["rows"]
                        select new DocumentInfo
                        {
                            Id = (string)row["id"],
                            Revision = (string)row["value"]["rev"]
                        }).ToList();
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        /// <summary>
        /// Get information on all the documents in the given database, with a limit for pagination
        /// </summary>
        /// <param name="db">The database name</param>
        /// <param name="docId">The id for the starting document</param>
        /// <param name="limit">The number of results</param>
        /// <returns>An array of DocInfo instances</returns>	
        public List<DocumentInfo> getAllDocuments(string db, string docId, long limit)
        {
            var url = getUrl() + "/" + db + "/_all_docs";

            if (!docId.Equals(String.Empty))
            {
                url += "?startkey_docid=" + docId;

                if (limit > 0)
                {
                    url += "&limit=" + limit.ToString(CultureInfo.InvariantCulture);
                }
            }
            else if (limit > 0)
            {
                url += "?limit=" + limit.ToString(CultureInfo.InvariantCulture);
            }

            var result = doRequest(url, "GET", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                var serializer = new JsonSerializerSettings();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());

                var ro = (JObject)JsonConvert.DeserializeObject(result.contentString, serializer);

                return (from JObject row in ro["rows"]
                        select new DocumentInfo
                        {
                            Id = (string)row["id"],
                            Revision = (string)row["value"]["rev"]
                        }).ToList();
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        #endregion

        #region " CountDocuments "

        /// <summary>
        /// Get the document count for the given database.
        /// </summary>
        /// <param name="db">The database name</param>
        /// <returns>The number of documents in the database</returns>
        public int countDocuments(string db)
        {
            // Get information about the database...
            var result = doRequest(getUrl() + "/" + db, "GET", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

                var count = int.Parse(ro["doc_count"].ToString());

                return count;
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        #endregion

        #region " DocumentExists "

        /// <summary>
        /// Checks if database exists
        /// </summary>
        /// <param name="db">The database name</param>
        /// <param name="docId"></param>
        /// <returns></returns>
        public bool documentExists(string db, string docId)
        {
            var ret = false;

            var result = doRequest(getUrl() + "/" + db + "/" + docId, "GET", false);
            //JObject ro = (JObject)JsonConvert.DeserializeObject(result);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                if (result.contentString.IndexOf("_id", StringComparison.Ordinal) != -1)
                {
                    ret = true;
                }
            }
            else
            {
                throw new InvalidServerResponseException("Invalid Server Response!", result);
            }

            return ret;
        }

        #endregion

        #region " CreateDocument "

        /// <summary>
        /// Create a new document. If the document has no ID field,
        /// it will be assigned one by the server.
        /// </summary>
        /// <param name="db">The database name</param>
        /// <param name="content">The document contents (JSON).</param>
        public DocumentInfo createDocument(string db, string content)
        {
            var result = doRequest(getUrl() + "/" + db, "POST", content, "application/json", false);
            //result	"{\"ok\":true,\"id\":\"4ac4e0e0f94b1e73e40403d1b3008263\",\"rev\":\"1-ce363ec7fbf74f9b1417ae0dfd605bfd\"}\n"	string

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                DocumentInfo docInfo;

                var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

                if (ro["ok"] != null)
                {
                    docInfo = new DocumentInfo
                    {
                        Id = (string)ro["id"],
                        Revision = (string)ro["rev"]
                    };
                }
                else
                {
                    throw new Exception((string)ro["reason"]);
                }

                return docInfo;
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        public DocumentInfo createDocument(string db, object content)
        {
            var result = doRequest(getUrl() + "/" + db, "POST", JsonConvert.SerializeObject(content), "application/json", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {

                DocumentInfo docInfo;

                var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

                if (ro["ok"] != null)
                {
                    docInfo = new DocumentInfo
                    {
                        Id = (string)ro["id"],
                        Revision = (string)ro["rev"]
                    };
                }
                else
                {
                    throw new Exception((string)ro["reason"]);
                }

                return docInfo;
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        /// <summary>
        /// Create bulk documents. If the documents have no ID field,
        /// it will be assigned one by the server.
        /// </summary>
        /// <param name="db">The database name</param>
        /// <param name="content">The document contents (JSON). (e.g: '{"docs":[{"key":"baz","name":"bazzel"}]}') </param>
        public void createBulkDocuments(string db, object content)
        {
            var converters = new JsonConverter[] { new IsoDateTimeConverter() };
            doRequest(getUrl() + "/" + db + "/_bulk_docs", "POST", JsonConvert.SerializeObject(content, converters), "application/json", false);
            //[{"ok":true,"id":"d5d9dbcd389c150c2ec13d2dcc01ccb2","rev":"1-e89734df382523dc0ce5878a3352437e"},{"ok":true,"id":"d5d9dbcd389c150c2ec13d2dcc01d461","rev":"1-e89734df382523dc0ce5878a3352437e"}]
        }

        #endregion

        #region " UpdateDocument "

        /// <summary>
        /// Create a new document. If the document has no ID field,
        /// it will be assigned one by the server.
        /// </summary>
        /// <param name="db">The database name</param>
        /// <param name="docId"></param>
        /// <param name="content">The document contents (JSON).</param>
        public DocumentInfo updateDocument(string db, string docId, string content)
        {
            var d = getDocumentAsJson(db, docId);

            var doc = (JObject)JsonConvert.DeserializeObject(d);

            var docRev = (string)doc["_rev"];

            var jo = (JObject)JsonConvert.DeserializeObject(content);

            if (jo["_id"] == null)
            {
                jo.Add("_id", docId);
            }
            else
            {
                jo["_id"] = docId;
            }

            if (jo["_rev"] == null)
            {
                jo.Add("_rev", docRev);
            }
            else
            {
                jo["_rev"] = docRev;
            }

			var result = doRequest(getUrl() + "/" + db, "POST", JsonConvert.SerializeObject(jo, new JsonConverter[] { new IsoDateTimeConverter() }), "application/json", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                DocumentInfo docInfo;

                var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

                if (ro["ok"] != null)
                {
                    docInfo = new DocumentInfo
                    {
                        Id = (string)ro["id"],
                        Revision = (string)ro["rev"]
                    };
                }
                else
                {
                    throw new Exception((string)ro["reason"]);
                }

                return docInfo;
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        public DocumentInfo updateDocument(string db, string docId, object content)
        {
            var d = getDocumentAsJson(db, docId);

            var doc = (JObject)JsonConvert.DeserializeObject(d);

            var docRev = (string)doc["_rev"];

            var jo = (JObject)(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(content)));

            if (jo["_id"] == null)
            {
                jo.Add("_id", docId);
            }
            else
            {
                jo["_id"] = docId;
            }

            if (jo["_rev"] == null)
            {
                jo.Add("_rev", docRev);
            }
            else
            {
                jo["_rev"] = docRev;
            }

			var result = doRequest(getUrl() + "/" + db, "POST", JsonConvert.SerializeObject(jo, new JsonConverter[] { new IsoDateTimeConverter() }), "application/json", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                DocumentInfo docInfo;

                var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

                if (ro["ok"] != null)
                {
                    docInfo = new DocumentInfo
                    {
                        Id = (string)ro["id"],
                        Revision = (string)ro["rev"]
                    };
                }
                else
                {
                    throw new Exception((string)ro["reason"]);
                }

                return docInfo;
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        #endregion

        #region " CopyDocument "

        /// <summary>
        /// This method allows you to duplicate the contents (and attachments) of a document 
        /// to a new document under a different document id without first retrieving it from CouchDB
        /// </summary>
        /// <param name="db"></param>
        /// <param name="docId"></param>
        /// <param name="newDocId"></param>
        /// <returns></returns>
        public DocumentInfo copyDocument(string db, string docId, string newDocId)
        {
            var headers = new WebHeaderCollection { { "Destination", newDocId } };

            var result = doRequest(getUrl() + "/" + db + "/" + docId, "COPY", headers, null, null, false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                DocumentInfo docInfo;
                var serializer = new JsonSerializerSettings();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());

                var ro = (JObject)JsonConvert.DeserializeObject(result.contentString, serializer);

                if ((string)ro["id"] == newDocId)
                {
                    docInfo = new DocumentInfo
                    {
                        Id = (string)ro["id"],
                        Revision = (string)ro["rev"]
                    };
                }
                else
                {
                    throw new Exception((string)ro["reason"]);
                }

                return docInfo;
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        #endregion

        #region " GetDocument "

        /// <summary>
        /// Get Document by its ID
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="docid"></param>
        /// <returns></returns>
        public T getDocument<T>(string db, string docid) where T : new()
        {
            var result = doRequest(getUrl() + "/" + db + "/" + docid, "GET", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                var serializer = new JsonSerializerSettings();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());

                var ro = JsonConvert.DeserializeObject(result.contentString, serializer);

                //ro.Remove("_id");
                //ro.Remove("_rev");

                var doc = new T();

                JsonConvert.PopulateObject(ro.ToString(), doc);

                return doc;
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        #endregion

        #region " GetDocumentAsJson "

        /// <summary>
        /// Get a document.
        /// </summary>
        /// <param name="db">The database name</param>
        /// <param name="docid">The document ID.</param>
        /// <returns>The document contents (JSON)</returns>
        public string getDocumentAsJson(string db, string docid)
        {
            var result = doRequest(getUrl() + "/" + db + "/" + docid, "GET", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                var serializer = new JsonSerializerSettings();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());

                var newResult = JsonConvert.DeserializeObject(result.contentString, serializer);
                return newResult.ToString();
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        /// <summary>
        /// Get a document.
        /// </summary>
        /// <param name="db">The database name</param>
        /// <param name="docid">The document ID.</param>
        /// <param name="startkey">The startkey or null not to use</param>
        /// <param name="endkey">The endkey or null not to use</param>
        /// <returns>The document contents (JSON)</returns>
        public string getDocumentAsJson(string db, string docid, string startkey, string endkey)
        {
            var url = getUrl() + "/" + db + "/" + docid;

            if (startkey != null)
            {
                url += "?startkey=" + HttpUtility.UrlEncode(startkey);
            }

            if (endkey != null)
            {
                if (startkey == null)
                    url += "?";
                else
                    url += "&";

                url += "endkey=" + HttpUtility.UrlEncode(endkey);
            }

            var result = doRequest(url, "GET", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                return result.contentString;
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        #endregion

        #region " DeleteDocument "

        /// <summary>
        /// Delete a document.
        /// </summary>
        /// <param name="db">The database name</param>
        /// <param name="docId">The document ID</param>
        public bool deleteDocument(string db, string docId)
        {
            var d = getDocumentAsJson(db, docId);

            var ro = (JObject)JsonConvert.DeserializeObject(d);

            var result = doRequest(getUrl() + "/" + db + "/" + docId + "?rev=" + (string)ro["_rev"], "DELETE", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                var jo = (JObject)JsonConvert.DeserializeObject(result.contentString);

                return jo["ok"] != null;
            }

            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        #endregion

        #region " DeleteDocuments "

        public void deleteDocuments(string db)
        {
            var bulkdocsResult = doRequest(getUrl() + "/" + db + "/_all_docs", "GET", false);

            if (bulkdocsResult.contentType.Contains("application/json"))
            {
                var ro = (JObject)JsonConvert.DeserializeObject(bulkdocsResult.contentString);

                var list = new JArray();

                foreach (JObject row in ro["rows"])
                {
                    row.Add("_id", row["id"]);
                    row.Remove("id");

                    row.Add("_rev", row["value"]["rev"]);
                    row.Remove("value");

                    row.Add("_deleted", true);

                    list.Add(row);
                }

                var bulkDeleteCommand = new JObject { { "docs", list } };

                //System.Diagnostics.Debug.WriteLine(bulkDeleteCommand);

                doRequest(getUrl() + "/" + db + "/_bulk_docs", "POST", JsonConvert.SerializeObject(bulkDeleteCommand), "application/json", false);

                //System.Diagnostics.Debug.WriteLine(result);
            }
            else
            {
                throw new InvalidServerResponseException("Invalid Server Response!", bulkdocsResult);
            }
        }

        public void deleteDocuments(string db, string bulkDeleteCommand)
        {
            doRequest(getUrl() + "/" + db + "/_bulk_docs", "POST", bulkDeleteCommand, "application/json", false);
            //System.Diagnostics.Debug.WriteLine(result);
        }

        #endregion

        #region " Attachments "

        #region " ExistsAttachment "

        public bool existsAttachment(string dbName, string docId, string attachmentName)
        {
            var result = false;

            var document = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, docId));

            if ((document["_attachments"] != null) && (document["_attachments"][attachmentName] != null))
            {
                result = true;
            }

            return result;
        }

        #endregion

        #region " SetInlineAttachment "

        public void setInlineAttachment(string dbName, string docId, string attachmentName, string attachmentContentType, string attachmentData)
        {
            var document = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, docId));

            if (document["_attachments"] != null)
            {
                if (document["_attachments"][attachmentName] != null)
                {
                    document["_attachments"][attachmentName]["content_type"] = attachmentContentType;
                    document["_attachments"][attachmentName]["data"] = attachmentData;
                }
                else
                {
                    ((JObject)document["_attachments"]).Add(attachmentName, new JObject());

                    ((JObject)document["_attachments"][attachmentName]).Add("content_type", attachmentContentType);
                    ((JObject)document["_attachments"][attachmentName]).Add("data", attachmentData);
                }
            }
            else
            {
                document["_attachments"] = new JObject();

                ((JObject)document["_attachments"]).Add(attachmentName, new JObject());

                ((JObject)document["_attachments"][attachmentName]).Add("content_type", attachmentContentType);
                ((JObject)document["_attachments"][attachmentName]).Add("data", attachmentData);
            }

            try
            {
                updateDocument(dbName, docId, document.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void setInlineAttachment(string dbName, string docId, string attachmentName, string attachmentContentType, byte[] attachmentData)
        {
            var document = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, docId));

            if (document["_attachments"] != null)
            {
                if (document["_attachments"][attachmentName] != null)
                {
                    document["_attachments"][attachmentName]["content_type"] = attachmentContentType;
                    document["_attachments"][attachmentName]["data"] = Convert.ToBase64String(attachmentData);
                }
                else
                {
                    ((JObject)document["_attachments"]).Add(attachmentName, new JObject());

                    ((JObject)document["_attachments"][attachmentName]).Add("content_type", attachmentContentType);
                    ((JObject)document["_attachments"][attachmentName]).Add("data", Convert.ToBase64String(attachmentData));
                }
            }
            else
            {
                document["_attachments"] = new JObject();

                ((JObject)document["_attachments"]).Add(attachmentName, new JObject());

                ((JObject)document["_attachments"][attachmentName]).Add("content_type", attachmentContentType);
                ((JObject)document["_attachments"][attachmentName]).Add("data", Convert.ToBase64String(attachmentData));
            }

            try
            {
                updateDocument(dbName, docId, document.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " GetInlineAttachment "

        public ServerResponse getInlineAttachment(string dbName, string docId, string attachmentName)
        {
            var result = doRequest(getUrl() + "/" + dbName + "/" + docId + "/" + HttpUtility.UrlEncode(attachmentName), "GET", false);

            return result;
        }

        #endregion

        #region " GetInlineAttachmentInfo "

        public List<InlineAttachmentInfo> getInlineAttachmentInfo(string dbName, string docId, string attachmentName)
        {
            try
            {
                var doc = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, docId));

                if (doc["_attachments"] != null)
                {
                    var result = new List<InlineAttachmentInfo>();

                    var jToken = doc["_attachments"].First;

                    while (jToken != null)
                    {
                        if (!String.IsNullOrEmpty(attachmentName))
                        {
                            if (((JProperty)jToken).Name.Equals(attachmentName))
                            {
                                result.Add(new InlineAttachmentInfo
                                               {
                                                   name = ((JProperty)jToken).Name,
                                                   contentType = jToken.First["content_type"].ToString().Trim(new[] { '"' }),
                                                   length = jToken.First["length"].ToString()
                                               });
                            }
                        }
                        else
                        {
                            result.Add(new InlineAttachmentInfo
                                           {
                                               name = ((JProperty)jToken).Name,
                                               contentType = jToken.First["content_type"].ToString().Trim(new[] { '"' }),
                                               length = jToken.First["length"].ToString()
                                           });
                        }

                        jToken = jToken.Next;
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return null;
        }

        #endregion

        #region " DeleteInlineAttachment "

        public void deleteInlineAttachment(string dbName, string docId, string attachmentName)
        {
            var document = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, docId));

            if (document["_attachments"] != null)
            {
                if (document["_attachments"][attachmentName] != null)
                {
                    ((JObject)document["_attachments"]).Remove(attachmentName);

                    if (((JObject)document["_attachments"]).Count == 0)
                    {
                        document.Remove("_attachments");
                    }
                }
                else
                {
                    throw new Exception("The Inline Attachment " + attachmentName + " does not exist!");
                }
            }
            else
            {
                throw new Exception("The Document " + docId + " does not contain any Inline Attachments!");
            }

            try
            {
                updateDocument(dbName, docId, document.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #endregion

        #region " Revisions "

        public JArray getDocumentRevisions(string db, string docId)
        {
            var result = doRequest(getUrl() + "/" + db + "/" + docId + "?revs_info=true", "GET", false);
            //result	"{\"ok\":true,\"id\":\"4ac4e0e0f94b1e73e40403d1b3008263\",\"rev\":\"1-ce363ec7fbf74f9b1417ae0dfd605bfd\"}\n"	string

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {

                var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);
                var revisions = new JArray();
                foreach (var revision in ro["_revs_info"])
                {
                    revisions.Add(revision);
                }
                return revisions;
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        #endregion

        #endregion

        #region " Design Document Functions "

        #region " GetAllDesignDocuments "

        /// <summary>
        /// Get information on all the design documents in the given database.
        /// </summary>
        /// <param name="db">The database name</param>
        /// <returns>An array of DocInfo instances</returns>	
        public List<DocumentInfo> getAllDesignDocuments(string db)
        {
            var result = doRequest(getUrl() + "/" + db + "/_all_docs?startkey=" + HttpUtility.UrlEncode("\"_design\"") + "&endkey=" + HttpUtility.UrlEncode("\"_design0\""), "GET", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

                return (from JObject row in ro["rows"]
                        select new DocumentInfo
                        {
                            Id = (string)row["id"],
                            Revision = (string)row["value"]["rev"]
                        }).ToList();
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        #endregion

        #region " CreateDesignDocument "

        /// <summary>
        /// Create Design Document. Design documents are a special type of CouchDB document 
        /// that contains application code. Because it runs inside a database, the application 
        /// API is highly structured. 
        /// More details: http://guide.couchdb.org/draft/design.html
        /// </summary>
        /// <param name="db"></param>
        /// <param name="name"></param>
        /// <param name="viewName"></param>
        /// <param name="map"></param>
        public DocumentInfo createDesignDocument(string db, string name, string viewName, string map)
        {
            var sb = new StringBuilder();
            var tw = new StringWriter(sb);

            using (var w = new JsonTextWriter(tw))
            {
                w.WriteStartObject();
                w.WritePropertyName("_id");
                w.WriteValue("_design/" + name);
                w.WritePropertyName("views");
                w.WriteStartObject();
                w.WritePropertyName(viewName);
                w.WriteStartObject();
                w.WritePropertyName("map");
                w.WriteValue(map);
                w.WriteEndObject();
                w.WriteEndObject();
                w.WriteEndObject();
            }

            var result = doRequest(getUrl() + "/" + db, "POST", sb.ToString(), "application/json", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                //System.Diagnostics.Debug.WriteLine(result);

                DocumentInfo docInfo;

                var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

                if (ro["ok"] != null)
                {
                    docInfo = new DocumentInfo
                    {
                        Id = (string)ro["id"],
                        Revision = (string)ro["rev"]
                    };
                }
                else
                {
                    throw new Exception((string)ro["reason"]);
                }

                return docInfo;
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        #endregion

        #region " CreateTemporaryView "

        /// <summary>
        /// Execute a temporary view and return the results.
        /// </summary>
        /// <param name="db">The database name</param>
        /// <param name="map">The javascript map function</param>
        /// <param name="reduce">The javascript reduce function or
        /// null if not required</param>
        /// <param name="startkey">The startkey or null not to use</param>
        /// <param name="endkey">The endkey or null not to use</param>
        /// <returns>The result (JSON format)</returns>
        public string createTemporaryView(string db, string map, string reduce, string startkey, string endkey)
        {
            // Generate the JSON view definition from the supplied map and optional reduce functions...
            var viewdef = "{ \"map\":\"" + map + "\"";

            if (reduce != null)
                viewdef += ",\"reduce\":\"" + reduce + "\"";

            viewdef += "}";

            var url = getUrl() + "/" + db + "/_temp_view";

            if (startkey != null)
            {
                url += "?startkey=" + HttpUtility.UrlEncode(startkey);
            }

            if (endkey != null)
            {
                if (startkey == null)
                    url += "?";
                else
                    url += "&";

                url += "endkey=" + HttpUtility.UrlEncode(endkey);
            }

            var result = doRequest(url, "POST", viewdef, "application/json", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                return result.contentString;
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        #endregion

        #region " CreateView "

        public string createView(string db, string viewName, string map, string reduce, string startkey, string endkey)
        {
            // Generate the JSON view definition from the supplied map and optional reduce functions...
            var viewdef = "{ \"map\":\"" + map + "\"";

            if (reduce != null)
                viewdef += ",\"reduce\":\"" + reduce + "\"";

            viewdef += "}";

            var url = getUrl() + "/" + db + "/" + viewName;

            if (startkey != null)
            {
                url += "?startkey=" + HttpUtility.UrlEncode(startkey);
            }

            if (endkey != null)
            {
                if (startkey == null)
                    url += "?";
                else
                    url += "&";

                url += "endkey=" + HttpUtility.UrlEncode(endkey);
            }

            var result = doRequest(url, "POST", viewdef, "application/json", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                return result.contentString;
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        #endregion

        #region " Show Functions "

        #region " ExistsShowFunction "

        public bool existsShowFunction(string dbName, string designDocument, string showFunctionName)
        {
            var result = false;

            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if ((dd["shows"] != null) && (dd["shows"][showFunctionName] != null))
            {
                result = true;
            }

            return result;
        }

        #endregion

        #region " GetShowFunction "

        public string getShowFunction(string dbName, string designDocument, string showFunctionName)
        {
            var content = sanitizeInOutput(getDocumentAsJson(dbName, designDocument));

            var dd = (JObject)JsonConvert.DeserializeObject(content);

            if ((dd["shows"] != null) && (dd["shows"][showFunctionName] != null))
            {
                return sanitizeOutOutput(dd["shows"][showFunctionName].ToString());
            }

            return String.Empty;
        }

        #endregion

        #region " SetShowFunction "

        public void setShowFunction(string dbName, string designDocument, string showFunctionName, string content)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["shows"] != null)
            {
                if (!String.IsNullOrEmpty(content))
                {
                    if (dd["shows"][showFunctionName] != null)
                    {
                        dd["shows"][showFunctionName] = content;
                    }
                    else
                    {
                        ((JObject)dd["shows"]).Add(showFunctionName, content);
                    }
                }
                else
                {
                    if (dd["shows"][showFunctionName] != null)
                    {
                        ((JObject)dd["shows"]).Remove(showFunctionName);
                    }
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(content))
                {
                    dd["shows"] = new JObject();

                    ((JObject)dd["shows"]).Add(showFunctionName, content);
                }
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " DeleteShowFunction "

        public void deleteShowFunction(string dbName, string designDocument, string showFunctionName)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["shows"] != null)
            {
                if (dd["shows"][showFunctionName] != null)
                {
                    ((JObject)dd["shows"]).Remove(showFunctionName);

                    if (((JObject)dd["shows"]).Count == 0)
                    {
                        dd.Remove("shows");
                    }
                }
                else
                {
                    throw new Exception("The Show Function " + showFunctionName + " does not exist!");
                }
            }
            else
            {
                throw new Exception("The Design Document " + designDocument + " does not contain any Show Functions!");
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " ExecuteShowFunction "

        public string executeShowFunction(string db, string designDocumentId, string showFunctionName, string documentId, List<KeyValuePair<string, string>> parameters)
        {
            var param = String.Empty;

            if (parameters.Count != 0)
            {
                param = parameters.Aggregate("?", (current, item) => current + (item.Key + "=" + item.Value + "&"));

                param = param.TrimEnd('&');
            }

            var result = doRequest(getUrl() + "/" + db + "/_design/" + designDocumentId + "/_show/" + showFunctionName + "/" +
                      documentId + param, "GET", false);

            return result.contentString;
        }

        #endregion

        #endregion

        #region " List Functions "

        #region " ExistsListFunction "

        public bool existsListFunction(string dbName, string designDocument, string listFunctionName)
        {
            var result = false;

            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if ((dd["lists"] != null) && (dd["lists"][listFunctionName] != null))
            {
                result = true;
            }

            return result;
        }

        #endregion

        #region " GetListFunction "

        public string getListFunction(string dbName, string designDocument, string listFunctionName)
        {
            var content = sanitizeInOutput(getDocumentAsJson(dbName, designDocument));

            var dd = (JObject)JsonConvert.DeserializeObject(content);

            if ((dd["lists"] != null) && (dd["lists"][listFunctionName] != null))
            {
                return sanitizeOutOutput(dd["lists"][listFunctionName].ToString());
            }

            return String.Empty;
        }

        #endregion

        #region " SetListFunction "

        public void setListFunction(string dbName, string designDocument, string listFunctionName, string content)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["lists"] != null)
            {
                if (!String.IsNullOrEmpty(content))
                {
                    if (dd["lists"][listFunctionName] != null)
                    {
                        dd["lists"][listFunctionName] = content;
                    }
                    else
                    {
                        ((JObject)dd["lists"]).Add(listFunctionName, content);
                    }
                }
                else
                {
                    if (dd["lists"][listFunctionName] != null)
                    {
                        ((JObject)dd["lists"]).Remove(listFunctionName);
                    }
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(content))
                {
                    dd["lists"] = new JObject();

                    ((JObject)dd["lists"]).Add(listFunctionName, content);
                }
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " DeleteListFunction "

        public void deleteListFunction(string dbName, string designDocument, string listFunctionName)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["lists"] != null)
            {
                if (dd["lists"][listFunctionName] != null)
                {
                    ((JObject)dd["lists"]).Remove(listFunctionName);

                    if (((JObject)dd["lists"]).Count == 0)
                    {
                        dd.Remove("lists");
                    }
                }
                else
                {
                    throw new Exception("The List Function " + listFunctionName + " does not exist!");
                }
            }
            else
            {
                throw new Exception("The Design Document " + designDocument + " does not contain any List Functions!");
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " GetDesignList "

        public List<T> getDesignList<T>(string db, string name, string viewName) where T : new()
        {
            var result = doRequest(getUrl() + "/" + db + "/_design/" + name + "/_list/" + viewName, "GET", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                var list = new List<T>();

                var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

                foreach (JObject row in ro["rows"])
                {
                    var o = (JObject)row["value"];

                    var doc = new T();
                    JsonConvert.PopulateObject(o.ToString(), doc);

                    list.Add(doc);
                }

                return list;
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        public List<T> getDesignList<T>(string db, string name, string listName, string viewName, List<KeyValuePair<string, string>> parameters) where T : new()
        {
            var param = String.Empty;

            if (parameters.Count != 0)
            {
                param = parameters.Aggregate("?", (current, item) => current + (item.Key + "=" + item.Value + "&"));

                param = param.TrimEnd('&');
            }

            var result = doRequest(getUrl() + "/" + db + "/_design/" + name + "/_list/" + listName + "/" + viewName + param, "GET", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                var list = new List<T>();

                var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

                foreach (JObject row in ro["rows"])
                {
                    var o = (JObject)row["value"];

                    var doc = new T();
                    JsonConvert.PopulateObject(o.ToString(), doc);

                    list.Add(doc);
                }

                return list;
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        public ServerResponse getDesignList(string db, string name, string viewName)
        {
            return doRequest(getUrl() + "/" + db + "/_design/" + name + "/_list/" + viewName, "GET", false);
        }

        public ServerResponse getDesignList(string db, string name, string listName, string viewName, List<KeyValuePair<string, string>> parameters)
        {
            var param = String.Empty;

            if (parameters.Count != 0)
            {
                param = parameters.Aggregate("?", (current, item) => current + (item.Key + "=" + item.Value + "&"));

                param = param.TrimEnd('&');
            }

            return doRequest(getUrl() + "/" + db + "/_design/" + name + "/_list/" + listName + "/" + viewName + param, "GET", false);
        }

        [Obsolete("getDesignListAsJson is deprecated, please use getDesignList instead.")]
        public string getDesignListAsJson(string db, string name, string viewName)
        {
            var result = doRequest(getUrl() + "/" + db + "/_design/" + name + "/_list/" + viewName, "GET", false);

            return result.contentString;
        }

        #endregion

        #endregion

        #region " View Functions "

        #region " ExistsViewFunction "

        public bool existsViewFunction(string dbName, string designDocument, string viewFunctionName)
        {
            var result = false;

            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if ((dd["views"] != null) && (dd["views"][viewFunctionName] != null))
            {
                result = true;
            }

            return result;
        }

        #endregion

        #region " SetViewFunction "

        public void setViewFunction(string dbName, string designDocument, string viewFunctionName, string mapContent, string reduceContent)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["views"] != null)
            {
                if (dd["views"][viewFunctionName] != null)
                {
                    if (!String.IsNullOrEmpty(mapContent))
                    {
                        if (dd["views"][viewFunctionName]["map"] != null)
                        {
                            dd["views"][viewFunctionName]["map"] = mapContent;
                        }
                        else
                        {
                            ((JObject)dd["views"][viewFunctionName]).Add("map", mapContent);
                        }
                    }
                    else
                    {
                        if (dd["views"][viewFunctionName]["map"] != null)
                        {
                            ((JObject)dd["views"][viewFunctionName]).Remove("map");
                        }
                    }

                    if (!String.IsNullOrEmpty(reduceContent))
                    {
                        if (dd["views"][viewFunctionName]["reduce"] != null)
                        {
                            dd["views"][viewFunctionName]["reduce"] = reduceContent;
                        }
                        else
                        {
                            ((JObject)dd["views"][viewFunctionName]).Add("reduce", reduceContent);
                        }
                    }
                    else
                    {
                        if (dd["views"][viewFunctionName]["reduce"] != null)
                        {
                            ((JObject)dd["views"][viewFunctionName]).Remove("reduce");
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(mapContent) || !String.IsNullOrEmpty(reduceContent))
                    {
                        ((JObject)dd["views"]).Add(viewFunctionName, String.Empty);

                        dd["views"][viewFunctionName] = new JObject();

                        if (!String.IsNullOrEmpty(mapContent))
                        {
                            ((JObject)dd["views"][viewFunctionName]).Add("map", mapContent);
                        }

                        if (!String.IsNullOrEmpty(reduceContent))
                        {
                            ((JObject)dd["views"][viewFunctionName]).Add("reduce", reduceContent);
                        }
                    }
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(mapContent) || !String.IsNullOrEmpty(reduceContent))
                {
                    dd["views"] = new JObject();

                    ((JObject)dd["views"]).Add(viewFunctionName, String.Empty);

                    dd["views"][viewFunctionName] = new JObject();

                    if (!String.IsNullOrEmpty(mapContent))
                    {
                        ((JObject)dd["views"][viewFunctionName]).Add("map", mapContent);
                    }

                    if (!String.IsNullOrEmpty(reduceContent))
                    {
                        ((JObject)dd["views"][viewFunctionName]).Add("reduce", reduceContent);
                    }
                }
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " DeleteViewFunction "

        public void deleteViewFunction(string dbName, string designDocument, string viewFunctionName)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["views"] != null)
            {
                if (dd["views"][viewFunctionName] != null)
                {
                    ((JObject)dd["views"]).Remove(viewFunctionName);

                    if (((JObject)dd["views"]).Count == 0)
                    {
                        dd.Remove("views");
                    }
                }
                else
                {
                    throw new Exception("The View Function " + viewFunctionName + " does not exist!");
                }
            }
            else
            {
                throw new Exception("The Design Document " + designDocument + " does not contain any View Functions!");
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " GetMapFunction "

        public string getMapFunction(string dbName, string designDocument, string viewFunctionName)
        {
            var content = sanitizeInOutput(getDocumentAsJson(dbName, designDocument));

            var dd = (JObject)JsonConvert.DeserializeObject(content);

            if ((dd["views"] != null) && (dd["views"][viewFunctionName] != null) && (dd["views"][viewFunctionName]["map"] != null))
            {
                return sanitizeOutOutput(dd["views"][viewFunctionName]["map"].ToString());
            }

            return String.Empty;
        }

        #endregion

        #region " GetReduceFunction "

        public string getReduceFunction(string dbName, string designDocument, string viewFunctionName)
        {
            var content = sanitizeInOutput(getDocumentAsJson(dbName, designDocument));
            var dd = (JObject)JsonConvert.DeserializeObject(content);

            if ((dd["views"] != null) && (dd["views"][viewFunctionName] != null) && (dd["views"][viewFunctionName]["reduce"] != null))
            {
                return sanitizeOutOutput(dd["views"][viewFunctionName]["reduce"].ToString());
            }

            return String.Empty;
        }

        #endregion

        #region " GetDesignView "

        public List<T> getDesignView<T>(string db, string name, string viewName) where T : new()
        {
            var result = doRequest(getUrl() + "/" + db + "/_design/" + name + "/_view/" + viewName, "GET", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                var list = new List<T>();

                var serializer = new JsonSerializerSettings();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;

                var ro = (JObject)JsonConvert.DeserializeObject(result.contentString, serializer);

                foreach (JObject row in ro["rows"])
                {
                    var o = (JObject)row["value"];
                    //o.Remove("_id");
                    //o.Remove("_rev");

                    var doc = new T();
                    JsonConvert.PopulateObject(o.ToString(), doc);

                    list.Add(doc);
                }

                return list;
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        public string getDesignViewAsJson(string db, string name, string viewName)
        {
            var result = doRequest(getUrl() + "/" + db + "/_design/" + name + "/_view/" + viewName, "GET", false);

            if (result.contentType.Contains("text/plain") || result.contentType.Contains("application/json"))
            {
                var serializer = new JsonSerializerSettings();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());

                var newResult = JsonConvert.DeserializeObject(result.contentString, serializer);
                return newResult.ToString();
            }
            throw new InvalidServerResponseException("Invalid Server Response!", result);
        }

        #endregion

        #endregion

        #region " Validate Functions "

        #region " ExistsValidateFunction "

        public bool existsValidateFunction(string dbName, string designDocument)
        {
            var result = false;

            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["validate_doc_update"] != null)
            {
                result = true;
            }

            return result;
        }

        #endregion

        #region " GetValidateFunction "

        public string getValidateFunction(string dbName, string designDocument)
        {
            var content = sanitizeInOutput(getDocumentAsJson(dbName, designDocument));
            var dd = (JObject)JsonConvert.DeserializeObject(content);

            if (dd["validate_doc_update"] != null)
            {
                return sanitizeOutOutput(dd["validate_doc_update"].ToString());
            }

            return String.Empty;
        }

        #endregion

        #region " SetValidateFunction "

        public void setValidateFunction(string dbName, string designDocument, string content)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["validate_doc_update"] != null)
            {
                if (!String.IsNullOrEmpty(content))
                {
                    dd["validate_doc_update"] = content;
                }
                else
                {
                    dd.Remove("validate_doc_update");
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(content))
                {
                    dd.Add("validate_doc_update", content);
                }
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " DeleteValidateFunction "

        public void deleteValidateFunction(string dbName, string designDocument)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["validate_doc_update"] != null)
            {
                dd.Remove("validate_doc_update");
            }
            else
            {
                throw new Exception("The Design Document " + designDocument + " does not contain a Validate Function!");
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #endregion

        #region " Rewrite Functions "

        #region " GetRewriteFunction "

        public string getRewriteFunction(string dbName, string designDocument, int rewriteFunctionNumber)
        {
            var content = sanitizeInOutput(getDocumentAsJson(dbName, designDocument));
            var dd = (JObject)JsonConvert.DeserializeObject(content);

            if ((dd["rewrites"] != null) && (dd["rewrites"][rewriteFunctionNumber] != null))
            {
                return sanitizeOutOutput(dd["rewrites"][rewriteFunctionNumber].ToString());
            }

            return String.Empty;
        }

        #endregion

        #region " SetRewriteFunction "

        public void setRewriteFunction(string dbName, string designDocument, int rewriteFunctionNumber, string content)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["rewrites"] != null)
            {
                if (!String.IsNullOrEmpty(content))
                {
                    if ((dd["rewrites"].Count() > rewriteFunctionNumber) && (dd["rewrites"][rewriteFunctionNumber] != null))
                    {
                        dd["rewrites"][rewriteFunctionNumber] = (JObject)JsonConvert.DeserializeObject(content);
                    }
                    else
                    {
                        ((JArray)dd["rewrites"]).Add((JObject)JsonConvert.DeserializeObject(content));
                    }
                }
                else
                {
                    if ((dd["rewrites"].Count() > rewriteFunctionNumber) && (dd["rewrites"][rewriteFunctionNumber] != null))
                    {
                        ((JArray)dd["rewrites"]).RemoveAt(rewriteFunctionNumber);
                    }
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(content))
                {
                    dd["rewrites"] = new JArray();

                    ((JArray)dd["rewrites"]).Add((JObject)JsonConvert.DeserializeObject(content));
                }
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " DeleteRewriteFunction "

        public void deleteRewriteFunction(string dbName, string designDocument, int rewriteFunctionNumber)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["rewrites"] != null)
            {
                if (dd["rewrites"][rewriteFunctionNumber] != null)
                {
                    ((JArray)dd["rewrites"]).RemoveAt(rewriteFunctionNumber);

                    if (((JArray)dd["rewrites"]).Count == 0)
                    {
                        dd.Remove("rewrites");
                    }
                }
                else
                {
                    throw new Exception("The Rewrite Function number " + rewriteFunctionNumber + " does not exist!");
                }
            }
            else
            {
                throw new Exception("The Design Document " + designDocument + " does not contain any Rewrite Functions!");
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #endregion

        #region " Filter Functions "

        #region " ExistsFilterFunction "

        public bool existsFilterFunction(string dbName, string designDocument, string filterFunctionName)
        {
            var result = false;

            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if ((dd["filters"] != null) && (dd["filters"][filterFunctionName] != null))
            {
                result = true;
            }

            return result;
        }

        #endregion

        #region " GetFilterFunction "

        public string getFilterFunction(string dbName, string designDocument, string filterFunctionName)
        {
            var content = sanitizeInOutput(getDocumentAsJson(dbName, designDocument));
            var dd = (JObject)JsonConvert.DeserializeObject(content);

            if ((dd["filters"] != null) && (dd["filters"][filterFunctionName] != null))
            {
                return sanitizeOutOutput(dd["filters"][filterFunctionName].ToString());
            }

            return String.Empty;
        }

        #endregion

        #region " SetFilterFunction "

        public void setFilterFunction(string dbName, string designDocument, string filterFunctionName, string content)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["filters"] != null)
            {
                if (!String.IsNullOrEmpty(content))
                {
                    if (dd["filters"][filterFunctionName] != null)
                    {
                        dd["filters"][filterFunctionName] = content;
                    }
                    else
                    {
                        (dd["filters"] as JObject).Add(filterFunctionName, content);
                    }
                }
                else
                {
                    if (dd["filters"][filterFunctionName] != null)
                    {
                        (dd["filters"] as JObject).Remove(filterFunctionName);
                    }
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(content))
                {
                    dd["filters"] = new JObject();

                    (dd["filters"] as JObject).Add(filterFunctionName, content);
                }
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " DeleteFilterFunction "

        public void deleteFilterFunction(string dbName, string designDocument, string filterFunctionName)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["filters"] != null)
            {
                if (dd["filters"][filterFunctionName] != null)
                {
                    ((JObject)dd["filters"]).Remove(filterFunctionName);

                    if (((JObject)dd["filters"]).Count == 0)
                    {
                        dd.Remove("filters");
                    }
                }
                else
                {
                    throw new Exception("The Filter Function " + filterFunctionName + " does not exist!");
                }
            }
            else
            {
                throw new Exception("The Design Document " + designDocument + " does not contain any Filter Functions!");
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #endregion

        #region " Index Functions "

        #region " ExistsIndexFunction "

        public bool existsIndexFunction(string dbName, string designDocument, string indexFunctionName)
        {
            var result = false;

            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if ((dd["indexes"] != null) && (dd["indexes"][indexFunctionName] != null))
            {
                result = true;
            }

            return result;
        }

        #endregion

        #region " GetIndexFunction "

        public string getIndexFunction(string dbName, string designDocument, string indexFunctionName)
        {
            var content = sanitizeInOutput(getDocumentAsJson(dbName, designDocument));

            var dd = (JObject)JsonConvert.DeserializeObject(content);

            if ((dd["indexes"] != null) && (dd["indexes"][indexFunctionName]["index"] != null))
            {
                return sanitizeOutOutput(dd["indexes"][indexFunctionName]["index"].ToString());
            }

            return String.Empty;
        }

        #endregion

        #region " SetIndexFunction "

        public void setIndexFunction(string dbName, string designDocument, string indexFunctionName, string content)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["indexes"] != null)
            {
                if (dd["indexes"][indexFunctionName] != null)
                {
                    if (!String.IsNullOrEmpty(content))
                        if (dd["indexes"][indexFunctionName]["index"] != null)
                            dd["indexes"][indexFunctionName]["index"] = content;
                        else
                            ((JObject)dd["indexes"][indexFunctionName]).Add("index", content);
                    else if (dd["indexes"][indexFunctionName]["index"] != null)
                        ((JObject)dd["indexes"][indexFunctionName]).Remove("index");
                }
                else
                {
                    if (!string.IsNullOrEmpty(content))
                    {
                        ((JObject)dd["indexes"]).Add(indexFunctionName, string.Empty);
                        dd["indexes"][indexFunctionName] = new JObject();

                        ((JObject)dd["indexes"][indexFunctionName]).Add("index", content);
                    }
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(content))
                {
                    dd["indexes"] = new JObject();
                    ((JObject)dd["indexes"]).Add(indexFunctionName, string.Empty);

                    dd["indexes"][indexFunctionName] = new JObject();

                    ((JObject)dd["indexes"][indexFunctionName]).Add("index", content);
                }
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " DeleteIndexFunction "

        public void deleteIndexFunction(string dbName, string designDocument, string indexFunctionName)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["indexes"] != null)
            {
                if (dd["indexes"][indexFunctionName] != null)
                {
                    ((JObject)dd["indexes"]).Remove(indexFunctionName);

                    if (((JObject)dd["indexes"]).Count == 0)
                    {
                        dd.Remove("indexes");
                    }
                }
                else
                {
                    throw new Exception("The Index Function " + indexFunctionName + " does not exist!");
                }
            }
            else
            {
                throw new Exception("The Design Document " + designDocument + " does not contain any Index Functions!");
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " GetDesignIndex "

        public ServerResponse getDesignIndex(string db, string name, string indexName, List<KeyValuePair<string, string>> parameters)
        {
            var param = String.Empty;

            if (parameters.Count != 0)
            {
                param = parameters.Aggregate("?", (current, item) => current + (item.Key + "=" + item.Value + "&"));

                param = param.TrimEnd('&');
            }

            return doRequest(getUrl() + "/" + db + "/_design/" + name + "/_search/" + indexName + param, "GET", false);
        }

        #endregion

        #endregion

        #region " Updates Functions "

        #region " ExistsUpdateFunction "

        public bool existsUpdateFunction(string dbName, string designDocument, string updateFunctionName)
        {
            var result = false;

            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if ((dd["updates"] != null) && (dd["updates"][updateFunctionName] != null))
            {
                result = true;
            }

            return result;
        }

        #endregion

        #region " GetUpdateFunction "

        public string getUpdateFunction(string dbName, string designDocument, string updateFunctionName)
        {
            var content = sanitizeInOutput(getDocumentAsJson(dbName, designDocument));

            var dd = (JObject)JsonConvert.DeserializeObject(content);

            if ((dd["updates"] != null) && (dd["updates"][updateFunctionName] != null))
            {
                return sanitizeOutOutput(dd["updates"][updateFunctionName].ToString());
            }

            return String.Empty;
        }

        #endregion

        #region " SetUpdatesFunction "

        public void setUpdateFunction(string dbName, string designDocument, string updateFunctionName, string content)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["updates"] != null)
            {
                if (!String.IsNullOrEmpty(content))
                {
                    if (dd["updates"][updateFunctionName] != null)
                    {
                        dd["updates"][updateFunctionName] = content;
                    }
                    else
                    {
                        ((JObject)dd["updates"]).Add(updateFunctionName, content);
                    }
                }
                else
                {
                    if (dd["updates"][updateFunctionName] != null)
                    {
                        ((JObject)dd["updates"]).Remove(updateFunctionName);
                    }
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(content))
                {
                    dd["updates"] = new JObject();

                    ((JObject)dd["updates"]).Add(updateFunctionName, content);
                }
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " DeleteUpdateFunction "

        public void deleteUpdateFunction(string dbName, string designDocument, string updateFunctionName)
        {
            var dd = (JObject)JsonConvert.DeserializeObject(getDocumentAsJson(dbName, designDocument));

            if (dd["updates"] != null)
            {
                if (dd["updates"][updateFunctionName] != null)
                {
                    ((JObject)dd["updates"]).Remove(updateFunctionName);

                    if (((JObject)dd["updates"]).Count == 0)
                    {
                        dd.Remove("updates");
                    }
                }
                else
                {
                    throw new Exception("The Update Function " + updateFunctionName + " does not exist!");
                }
            }
            else
            {
                throw new Exception("The Design Document " + designDocument + " does not contain any Update Functions!");
            }

            try
            {
                updateDocument(dbName, designDocument, dd.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " ExecuteUpdateFunction "

        public string executeUpdateFunction(string db, string designDocumentId, string updateFunctionName, string documentId, List<KeyValuePair<string, string>> parameters)
        {
            var param = String.Empty;

            if (parameters.Count != 0)
            {
                param = parameters.Aggregate("?", (current, item) => current + (item.Key + "=" + item.Value + "&"));

                param = param.TrimEnd('&');
            }

            var result = doRequest(getUrl() + "/" + db + "/_design/" + designDocumentId + "/_update/" + updateFunctionName + "/" +
                      documentId + param, "PUT", false);

            return result.contentString;
        }

        #endregion

        #endregion

        #endregion
    }
}