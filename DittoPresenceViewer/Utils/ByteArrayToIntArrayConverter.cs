using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DittoTools.PresenceViewer.Utils
{
    internal class ByteArrayToIntArrayConverter : JsonConverter<byte[]>
    {
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException("Deserialization is not implemented.");
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var byteValue in value)
            {
                writer.WriteNumberValue(byteValue);
            }
            writer.WriteEndArray();
        }
    }

}
