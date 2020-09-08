using maskx.AzurePolicy.Services;
using maskx.AzurePolicy.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace maskx.AzurePolicy.Definitions
{
    public class Effect
    {
        [JsonPropertyName("effect")]
        public string Name { get; set; }
        [JsonIgnore()]
        public int Priority { get { return EffectService.GetPriority(this.Name); } }
        [JsonConverter(typeof(RawTextConverter))]
        public string Details { get; set; }
        public static implicit operator Effect(string rawString)
        {
            var effect = JsonSerializer.Deserialize<Effect>(rawString, SerializerOptions.Default);
            return effect;
        }
    }
}