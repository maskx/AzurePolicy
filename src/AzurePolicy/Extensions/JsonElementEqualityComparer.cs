using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace maskx.AzurePolicy.Extensions
{
    public class JsonElementEqualityComparer : IEqualityComparer<JsonElement>
    {
        public bool Equals(JsonElement x, JsonElement y)
        {
            return x.IsEqual(y);
        }

        public int GetHashCode(JsonElement obj)
        {
            int rtv = 0;
            if (obj.ValueKind is JsonValueKind.Array)
            {
                rtv = obj.EnumerateArray().Sum((e) =>
                {
                    return GetHashCode(e);
                });
            }
            else if (obj.ValueKind == JsonValueKind.Object)
            {
                rtv = obj.EnumerateObject().Sum((p) =>
                {
                    return p.Name.GetHashCode() + GetHashCode(p.Value);
                });
            }
            else
            {
                rtv = obj.GetRawText().GetHashCode();
            }
            return rtv;
        }
    }
}
