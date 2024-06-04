﻿using System;
namespace DittoTools.Heartbeat
{
    public class DittoHeartbeatConfig
    {
        public string Id { get; private set; }

        public int SecondsInterval { get; private set; }

        public Dictionary<string, object>? MetaData { get; private set; }

        public DittoHeartbeatConfig(string id, int secondsInterval, Dictionary<string, object>? metaData = null)
        {
            Id = id;
            SecondsInterval = secondsInterval;
            MetaData = metaData;
        }

        private DittoHeartbeatConfig()
        {
        }

        public static DittoHeartbeatConfig MockConfig => new DittoHeartbeatConfig()
        {
            Id = Guid.NewGuid().ToString(),
            SecondsInterval = 10,
            MetaData = new Dictionary<string, object>()
            {
                { "metadata-key1", "metadata-value1" },
                { "metadata-key2", "metadata-value2" }
            }
        };
    }
}

