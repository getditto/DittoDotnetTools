using System;
using System.Text.Json.Serialization;

namespace DittoTools.Heartbeat
{
    public class DittoHeartbeatInfo
    {
        [JsonPropertyName("_id")]
        public string Id { get; internal set; }

        [JsonPropertyName("lastUpdated")]
        public string LastUpdated { get; internal set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; internal set;  }

        [JsonPropertyName("secondsInterval")]
        public int SecondsInterval { get; internal set;  }

        [JsonPropertyName("presenceSnapshotDirectlyConnectedPeersCount")]
        public int PresenceSnapshotDirectlyConnectedPeersCount { get; internal set; }

        [JsonPropertyName("presenceSnapshotDirectlyConnectedPeers")]
        public Dictionary<string, object> PresenceSnapshotDirectlyConnectedPeers { get; internal set; }

        [JsonPropertyName("sdk")]
        public string Sdk { get; internal set; }

        [JsonPropertyName("_schema")]
        public string Schema { get; internal set; }

        [JsonPropertyName("peerKey")]
        public string PeerKey { get; internal set; }

        internal DittoHeartbeatInfo()
        {
        }

        public DittoHeartbeatInfo(string id, string lastUpdated, Dictionary<string, object>? metaData, int secondsInterval,
            int presenceSnapshotDirectlyConnectedPeersCount, Dictionary<string, object> presenceSnapshotDirectlyConnectedPeers,
            string sdk, string schema, string peerKey)
        {
            Id = id;
            LastUpdated = lastUpdated;
            Metadata = metaData;
            SecondsInterval = secondsInterval;
            PresenceSnapshotDirectlyConnectedPeersCount = presenceSnapshotDirectlyConnectedPeersCount;
            PresenceSnapshotDirectlyConnectedPeers = presenceSnapshotDirectlyConnectedPeers;
            Sdk = sdk;
            Schema = schema;
            PeerKey = peerKey;
        }

        public override string ToString()
        {
            return $"id: {Id}\n" +
                $"secondsInterval : {SecondsInterval}\n" +
                $"lastUpdated: {LastUpdated}\n" +
                $"metadata: {Metadata}\n" +
                $"presenceSnapshotDirectlyConnectedPeersCount: {PresenceSnapshotDirectlyConnectedPeersCount}\n";
        }
    }
}

