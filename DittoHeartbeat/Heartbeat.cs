using DittoSDK;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DittoTools_Heartbeat {
    public class DittoHeartbeatConfig
    {
        public string Id { get; }
        public int SecondsInterval { get; }
        public Dictionary<string, object>? MetaData { get; }
        public DittoHeartbeatConfig(string id, int secondsInterval, Dictionary<string, object>? metaData = null)
        {
            Id = id;
            SecondsInterval = secondsInterval;
            MetaData = metaData;
        }
    }

    public class DittoHeartbeatInfo
    {
        public string Id { get; }
        public string LastUpdated { get; }
        public Dictionary<string, object>? MetaData { get; }
        public int SecondsInterval { get; }
        public int PresenceSnapshotDirectlyConnectedPeersCount { get; }
        public Dictionary<string, object> PresenceSnapshotDirectlyConnectedPeers { get; }
        public string Sdk { get; }
        public string Schema { get; }
        public string PeerKey { get; }

        public DittoHeartbeatInfo(string id, string lastUpdated, Dictionary<string, object>? metaData, int secondsInterval, 
            int presenceSnapshotDirectlyConnectedPeersCount, Dictionary<string, object> presenceSnapshotDirectlyConnectedPeers, 
            string sdk, string schema, string peerKey)
        {
            Id = id;
            LastUpdated = lastUpdated;
            MetaData = metaData;
            SecondsInterval = secondsInterval;
            PresenceSnapshotDirectlyConnectedPeersCount = presenceSnapshotDirectlyConnectedPeersCount;
            PresenceSnapshotDirectlyConnectedPeers = presenceSnapshotDirectlyConnectedPeers;
            Sdk = sdk;
            Schema = schema;
            PeerKey = peerKey;
        }
    }


    public class DittoHeartbeat
    {
        private Timer? timer;
        private DittoSyncSubscription? hbSubscription;

        public void StartHeartbeat(Ditto ditto, DittoHeartbeatConfig config, Action<DittoHeartbeatInfo>? callback)
        {
            if(hbSubscription == null) {
                hbSubscription = ditto.Sync.RegisterSubscription($"SELECT * FROM {Constants.collectionName}");
            }

            var presenceData = GetPeers(ditto);
            string lastUpdated = ISO8601DateFormatter.GetISODate();

            DittoHeartbeatInfo info = new DittoHeartbeatInfo(
                id: config.Id,
                lastUpdated: lastUpdated,
                metaData: config.MetaData,
                secondsInterval: config.SecondsInterval,
                presenceSnapshotDirectlyConnectedPeersCount: presenceData.Count,
                presenceSnapshotDirectlyConnectedPeers: GetConnections(presenceData, ditto),
                sdk: Ditto.Version,
                schema: Constants.schema,
                peerKey: ditto.Presence.Graph.LocalPeer.PeerKeyString
            );

            AddToCollection(info, config, ditto);

            // Invoke the callback with the info variable
            callback?.Invoke(info);

            // Set up a timer to run the function at the specified interval
            timer = new Timer(_ =>
            {
                StartHeartbeat(ditto, config, callback);
            }, null, config.SecondsInterval * 1000, Timeout.Infinite);
        }

        public void StopHeartbeat()
        {
            // Stop the timer
            timer?.Change(Timeout.Infinite, Timeout.Infinite);

            if(hbSubscription != null) {
                hbSubscription.Cancel();
            }
        }

        private static List<DittoPeer> GetPeers(Ditto ditto) 
        {
            return ditto.Presence.Graph.RemotePeers;
        }

        private static Dictionary<string, object> GetConnections(List<DittoPeer>? presenceSnapshotDirectlyConnectedPeers, Ditto ditto)
        {
            Dictionary<string, object> connectionsMap = new Dictionary<string, object>();

            if (presenceSnapshotDirectlyConnectedPeers != null)
            {
                foreach (var connection in presenceSnapshotDirectlyConnectedPeers)
                {
                    var connectionsTypeMap = GetConnectionTypeCount(connection);

                    var connectionMap = new Dictionary<string, object>
                    {
                        { "deviceName", connection.DeviceName },
                        { "sdk", Ditto.Version },
                        { "isConnectedToDittoCloud", connection.IsDittoCloudConnected },
                        { "bluetooth", connectionsTypeMap["bt"] },
                        { "p2pWifi", connectionsTypeMap["p2pWifi"] },
                        { "lan", connectionsTypeMap["lan"] }
                    };

                    connectionsMap[connection.PeerKeyString] = connectionMap;
                }
            }

            return connectionsMap;
        }
        private static Dictionary<string, int> GetConnectionTypeCount(DittoPeer connection)
        {
            var bt = 0;
            var p2pWifi = 0;
            var lan = 0;

            foreach (var conn in connection.Connections)
            {
                switch (conn.ConnectionType)
                {
                    case DittoConnectionType.Bluetooth:
                        bt++;
                        break;
                    case DittoConnectionType.P2PWiFi:
                        p2pWifi++;
                        break;
                    case DittoConnectionType.AccessPoint:
                    case DittoConnectionType.WebSocket:
                        lan++;
                        break;
                }
            }

            return new Dictionary<string, int>
            {
                { "bt", bt },
                { "p2pWifi", p2pWifi },
                { "lan", lan }
            };
        }

        private static void AddToCollection(DittoHeartbeatInfo info, DittoHeartbeatConfig config, Ditto ditto)
        {
            var metaData = config?.MetaData ?? new Dictionary<string, object>();
            var doc = new Dictionary<string, object>
            {
                { "_id", info.Id },
                { "secondsInterval", info.SecondsInterval },
                { "presenceSnapshotDirectlyConnectedPeersCount", info.PresenceSnapshotDirectlyConnectedPeersCount },
                { "lastUpdated", info.LastUpdated },
                { "presenceSnapshotDirectlyConnectedPeers", info.PresenceSnapshotDirectlyConnectedPeers },
                { "metadata", metaData },
                { "sdk", info.Sdk },
                { "_schema", info.Schema },
                { "peerKey", info.PeerKey }
            };

            ditto.Store.Collection(Constants.collectionName).Upsert(doc);
        }
    }
}