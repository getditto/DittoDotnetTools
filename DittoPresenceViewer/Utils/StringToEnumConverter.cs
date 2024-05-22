using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ditto.PresenceViewer.Utils
{
    public class StringToEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string enumString = reader.GetString();
                if (Enum.TryParse<T>(enumString, true, out var enumValue))
                {
                    return enumValue;
                }
            }

            throw new JsonException($"Unable to convert \"{reader.GetString()}\" to enum \"{typeof(T)}\".");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}

