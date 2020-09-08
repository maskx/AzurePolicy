using maskx.ARMOrchestration.Extensions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace maskx.AzurePolicy.Utilities
{
    public class RawTextConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            return jsonDoc.RootElement.GetRawText();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteRawString(value);
        }
    }
}
