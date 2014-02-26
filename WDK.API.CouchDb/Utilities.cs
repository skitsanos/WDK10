using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WDK.API.CouchDb
{
    public class Utilities
    {
        #region " Replicate "

        public static bool replicate(NamedReplicationConfiguration config, ReplicationType type, bool createTarget)
        {
            var endpoint = new CouchDbEndpoint();
            switch (type)
            {
                case ReplicationType.PUSH:

                    endpoint = new CouchDbEndpoint
                    {
                        host = config.source.host,
                        port = config.source.port,
                        db = config.source.db,
                        useSsl = config.source.useSsl,
                        username = config.source.username,
                        password = config.source.password
                    };
                    break;

                case ReplicationType.PULL:

                    endpoint = new CouchDbEndpoint
                    {
                        host = config.target.host,
                        port = config.target.port,
                        db = config.target.db,
                        useSsl = config.target.useSsl,
                        username = config.target.username,
                        password = config.target.password
                    };
                    break;
            }

            var content = new JObject
                {
                    {"source", config.source.getUrl() + "/" + config.source.db.ToLower()},
                    {"target", config.target.getUrl() + "/" + config.target.db.ToLower()}
                };

            if (createTarget)
                content.Add("create_target", true);

            if (!string.IsNullOrEmpty(config.proxyAddress))
                content.Add("proxy", config.proxyAddress + ":" + config.proxyPort);

            content.Add("continuous", config.continuous);

            if (!string.IsNullOrEmpty(config.filter))
                content.Add("filter", config.filter);

            if (config.docs != null)
            {
                var array = new JArray();
                foreach (var doc in config.docs)
                {
                    array.Add(doc);
                }
                content.Add("doc_ids", array);
            }

            var req = endpoint.getRequest("/_replicate", "POST", JsonConvert.SerializeObject(content));
            var result = endpoint.getResponse(req, false);

            /**
             * {"ok":true,"_local_id":"0a81b645497e6270611ec3419767a584+continuous+create_target"}
             */
            var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);
            return ro["ok"] != null && (bool)ro["ok"];
        }

        public static JObject keepDeletedReplication(FilteredReplicationConfiguration config, bool createTarget)
        {
            var endpoint = new CouchDbEndpoint
            {
                host = config.source.host,
                port = config.source.port,
                db = config.source.db,
                useSsl = config.source.useSsl,
                username = config.source.username,
                password = config.source.password
            };

            var content = new JObject
                {
                    {"source", config.source.getUrl() + "/" + config.source.db.ToLower()},
                    {"target", config.target.getUrl() + "/" + config.target.db.ToLower()}
                };

            if (createTarget)
                content.Add("create_target", true);

            if (!string.IsNullOrEmpty(config.proxyAddress))
                content.Add("proxy", config.proxyAddress + ":" + config.proxyPort);

            content.Add("continuous", config.continuous);

            if (!string.IsNullOrEmpty(config.filter))
                content.Add("filter", config.filter);

            var req = endpoint.getRequest("/_replicate", "POST", JsonConvert.SerializeObject(content));
            var result = endpoint.getResponse(req, false);

            /**
             * {"ok":true,"_local_id":"0a81b645497e6270611ec3419767a584+continuous+create_target"}
             */
            return (JObject)JsonConvert.DeserializeObject(result.contentString);
        }

        #endregion

        #region " Compaction "

        public static bool databaseCompaction(CompactionConfiguration configuration)
        {
            var req = configuration.endpoint.getRequest("/" + configuration.endpoint.db + "/_compact", "POST");
            var result = configuration.endpoint.getResponse(req, false);

            var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

            return ro["ok"] != null && (bool)ro["ok"];
        }

        public static bool databaseCompactionStatus(CompactionConfiguration configuration)
        {
            var req = configuration.endpoint.getRequest("/" + configuration.endpoint.db, "GET");
            var result = configuration.endpoint.getResponse(req, false);

            var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

            return ro["compact_running"] != null && (bool)ro["compact_running"];
        }

        public static bool viewsCompaction(CompactionConfiguration configuration, string designDocumentName)
        {
            var req = configuration.endpoint.getRequest("/" + configuration.endpoint.db + "/_compact/" + designDocumentName, "POST");
            var result = configuration.endpoint.getResponse(req, false);

            var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

            return ro["ok"] != null && (bool)ro["ok"];
        }

        public static bool viewsCompactionStatus(CompactionConfiguration configuration, string documentName)
        {
            var req = configuration.endpoint.getRequest("/" + configuration.endpoint.db + "/_design/" + documentName + "/_info", "GET");
            var result = configuration.endpoint.getResponse(req, false);

            var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);
            var view_index = ro["view_index"];

            return view_index["compact_running"] != null && (bool)view_index["compact_running"];
        }

        #endregion

        #region " Views Cleanup

        public static bool viewsCleanup(CompactionConfiguration configuration)
        {
            var req = configuration.endpoint.getRequest("/" + configuration.endpoint.db + "/_view_cleanup", "POST");
            var result = configuration.endpoint.getResponse(req, false);

            var ro = (JObject)JsonConvert.DeserializeObject(result.contentString);

            return ro["ok"] != null && (bool)ro["ok"];
        }

        #endregion
    }
}
