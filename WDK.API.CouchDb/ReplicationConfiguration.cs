namespace WDK.API.CouchDb
{
    #region " Replicate "

    public class ReplicationConfiguration
    {
        public CouchDbEndpoint source = new CouchDbEndpoint();
        public CouchDbEndpoint target = new CouchDbEndpoint();

        public bool continuous;

        public string proxyAddress;
        public int proxyPort;
    }

    public class FilteredReplicationConfiguration : ReplicationConfiguration
    {
        public string filter;
    }

    public class NamedReplicationConfiguration : FilteredReplicationConfiguration
    {
        public string[] docs;
    }

    public enum ReplicationType
    {
        PUSH,
        PULL
    }

    #endregion

    #region " Compaction "

    public class CompactionConfiguration
    {
        public CouchDbEndpoint endpoint = new CouchDbEndpoint();
    }

    #endregion
}
