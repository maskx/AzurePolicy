using System.Text.Json;
using System.Text.Json.Serialization;

namespace maskx.AzurePolicy.Utilities
{
    public static class SerializerOptions
    {
        static SerializerOptions()
        {
            Default = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            Default.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        }
        public static JsonSerializerOptions Default { get; private set; }
    }
}
