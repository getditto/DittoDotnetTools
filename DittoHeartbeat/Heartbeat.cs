using DittoSDK;
using DittoSDK.Sync;
using DittoSDK.Transport;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DittoTools.Heartbeat
{
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
                metaData: config.Metadata,
                secondsInterval: config.SecondsInterval,
                presenceSnapshotDirectlyConnectedPeersCount: presenceData.Count,
                presenceSnapshotDirectlyConnectedPeers: GetConnections(presenceData, ditto),
                sdk: Ditto.Version,
                schema: Constants.schema,
                peerKey: ditto.Presence.Graph.LocalPeer.PeerKey
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
                    var connectionTypes = CountConnectionTypes(connection);

                    var connectionMap = new Dictionary<string, object>
                    {
                        { "deviceName", connection.DeviceName },
                        { "sdk", Ditto.Version },
                        // SDK v4 exposed this connection state as
                        // IsConnectedToDittoCloud; SDK v5 exposes the same state
                        // as IsConnectedToDittoServer. Heartbeat documents are
                        // exchanged between independently versioned tools, so the
                        // stored key remains isConnectedToDittoCloud. Changing it
                        // would make existing readers miss the connection state.
                        { "isConnectedToDittoCloud", connection.IsConnectedToDittoServer },
                        { "bluetooth", connectionTypes.GetValueOrDefault(DittoConnectionType.Bluetooth.ToString()) },
                        { "p2pWifi", connectionTypes.GetValueOrDefault(DittoConnectionType.P2PWiFi.ToString()) },
                        // The SDK enum calls LAN `AccessPoint`. WebSocket is a
                        // separate transport in the open-ended connectionTypes map.
                        { "lan", connectionTypes.GetValueOrDefault(DittoConnectionType.AccessPoint.ToString()) },
                        { "connectionTypes", connectionTypes }
                    };

                    connectionsMap[connection.PeerKey] = connectionMap;
                }
            }

            return connectionsMap;
        }

        private static Dictionary<string, int> CountConnectionTypes(DittoPeer peer)
        {
            var connectionTypes = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var connection in peer.Connections)
            {
                var name = connection.ConnectionType.ToString();
                connectionTypes[name] = connectionTypes.GetValueOrDefault(name) + 1;
            }

            return connectionTypes;
        }

        private static void AddToCollection(DittoHeartbeatInfo info, DittoHeartbeatConfig config, Ditto ditto)
        {
            var metaData = config?.Metadata ?? new Dictionary<string, object>();
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

            // Capture the Task so that store errors are observed rather than
            // silently swallowed as unobserved task exceptions.
            ditto.Store.ExecuteAsync(
                $"INSERT INTO {Constants.collectionName} DOCUMENTS (:doc) ON ID CONFLICT DO UPDATE",
                new Dictionary<string, object> { ["doc"] = doc })
                .ContinueWith(
                    t => System.Diagnostics.Debug.WriteLine($"[DittoHeartbeat] Store.ExecuteAsync failed: {t.Exception}"),
                    TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
