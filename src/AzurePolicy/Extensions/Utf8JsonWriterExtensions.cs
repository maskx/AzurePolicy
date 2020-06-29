using maskx.ARMOrchestration.Functions;
using maskx.AzurePolicy.Functions;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace maskx.AzurePolicy.Extensions
{
    public static class Utf8JsonWriterExtensions
    {
        public static void WriteElement(this Utf8JsonWriter writer, JsonElement element, Dictionary<string, object> context, PolicyFunction function)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Undefined:
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    element.WriteTo(writer);
                    break;

                case JsonValueKind.Object:
                    writer.WriteStartObject();
                    foreach (var p in element.EnumerateObject())
                    {
                        writer.WriteProperty(p, context, function);
                    }
                    writer.WriteEndObject();
                    break;

                case JsonValueKind.Array:
                    writer.WriteStartArray();
                    foreach (var a in element.EnumerateArray())
                    {
                        writer.WriteElement(a, context, function);
                    }
                    writer.WriteEndArray();
                    break;

                case JsonValueKind.String:
                    var r = function.Evaluate(element.GetString(), context);
                    if (r is JsonValue j)
                    {
                        using var doc = JsonDocument.Parse(j.ToString());
                        doc.RootElement.WriteTo(writer);
                    }
                    else if (r is bool b)
                        writer.WriteBooleanValue(b);
                    else if (r is string s)
                        writer.WriteStringValue(s);
                    else if (r is Int32 i)
                        writer.WriteNumberValue(i);
                    else
                        writer.WriteNullValue();
                    break;

                default:
                    break;
            }
        }
        public static (bool Result, string Message) WriteProperty(this Utf8JsonWriter writer, JsonProperty property, Dictionary<string, object> context, PolicyFunction function)
        {
            writer.WritePropertyName(property.Name);
            writer.WriteElement(property.Value, context, function);
            return (true, string.Empty);
        }
    }
}
