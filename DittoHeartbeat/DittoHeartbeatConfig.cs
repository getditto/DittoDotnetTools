using System;
namespace DittoTools.Heartbeat
{
    [System.Obsolete("This tool is deprecated and will be removed in the next major DittoDotnetTools release.")]
    public class DittoHeartbeatConfig
    {
        public string Id { get; private set; }

        public int SecondsInterval { get; private set; }

        public Dictionary<string, object>? Metadata { get; private set; }

        public DittoHeartbeatConfig(string id, int secondsInterval, Dictionary<string, object>? metaData = null)
        {
            Id = id;
            SecondsInterval = secondsInterval;
            Metadata = metaData;
        }

        private DittoHeartbeatConfig()
        {
        }

        public static DittoHeartbeatConfig MockConfig => new DittoHeartbeatConfig()
        {
            Id = Guid.NewGuid().ToString(),
            SecondsInterval = 10,
            Metadata = new Dictionary<string, object>()
            {
                { "metadata-key1", "metadata-value1" },
                { "metadata-key2", "metadata-value2" }
            }
        };
    }
}

