using maskx.ARMOrchestration.Functions;
using maskx.AzurePolicy.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace maskx.AzurePolicy.Extensions
{
    public static class JsonElementExtensions
    {
        public static bool IsEqual(this JsonElement self, JsonElement target)
        {
            if (self.ValueKind != target.ValueKind)
                return false;
            switch (self.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var item in self.EnumerateObject())
                    {
                        if (!target.TryGetProperty(item.Name, out JsonElement e))
                            return false;
                        if (!item.Value.IsEqual(e))
                            return false;
                    }
                    return true;

                case JsonValueKind.Array:
                    if (self.GetArrayLength() != target.GetArrayLength())
                        return false;
                    for (int i = 0; i < self.GetArrayLength(); i++)
                    {
                        if (!self[i].IsEqual(target[i]))
                            return false;
                    }
                    return true;

                case JsonValueKind.String:
                case JsonValueKind.Number:
                    return self.GetRawText() == target.GetRawText();
            }
            return false;
        }

        public static IEnumerable<object> Intersect(this JsonElement self, JsonElement target)
        {
            if (self.ValueKind == JsonValueKind.Array)
            {
                self.EnumerateArray().Intersect(target.EnumerateArray(), new JsonElementEqualityComparer());
            }
            else if (self.ValueKind == JsonValueKind.Object)
            {
                self.EnumerateObject().Intersect(target.EnumerateObject(), new JsonPropertyEqualityComparer());
            }
            return null;
        }
        public static JsonElement GetElement(this JsonElement self, string path)
        {
            JsonElement e = self;
            foreach (var p in path.Split("/"))
            {
                if (!e.TryGetProperty(p, out e))
                {
                    throw new Exception($"element does not have a node named: {p}");
                }
            }
            return e;
        }
        public static JsonElement GetElementDot(this JsonElement self, string path)
        {
            JsonElement e = self;
            foreach (var p in path.Split("."))
            {
                if (!e.TryGetProperty(p, out e))
                {
                    throw new Exception($"element does not have a node named: {p}");
                }
            }
            return e;
        }
        public static JsonElement GetElementDotWithoutException(this JsonElement self, string path)
        {
            JsonElement e = self;
            foreach (var p in path.Split("."))
            {
                if (!e.TryGetProperty(p, out e))
                {
                    return default;// TODO: 需要测试，当读取path不存在时的行为
                }
            }
            return e;
        }
        public static List<object> GetElements(this JsonElement self, List<string> path, ARMFunctions functions, Dictionary<string, object> context)
        {
            List<object> list = new List<object>();
            JsonElement e = self;
            var p = path[0];
            path.RemoveAt(0);
            if (p.EndsWith("[*]"))
            {
                if (e.TryGetProperty(p.Remove(p.Length - 3), out JsonElement e2))
                {
                    foreach (var item in e2.EnumerateArray())
                    {
                        if (path.Count == 0)
                            list.Add(item.GetEvaluatedValue(functions, context));
                        else
                            list.AddRange(item.GetElements(path, functions, context));
                    }
                }
            }
            else
            {
                if (e.TryGetProperty(p, out JsonElement e1))
                {

                    if (path.Count == 0)
                        list.Add(e1.GetEvaluatedValue(functions, context));
                    else
                        list.AddRange(e1.GetElements(path, functions, context));
                }
            }
            return list;
        }

        public static object GetEvaluatedValue(this JsonElement self, ARMFunctions functions, Dictionary<string, object> context)
        {
            switch (self.ValueKind)
            {
                case JsonValueKind.String:
                    return functions.Evaluate(self.GetString(), context);
                case JsonValueKind.Number:
                    return self.GetInt32();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return self.GetBoolean();
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    return new JsonValue(self.GetRawText());
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                default:
                    return null;
            }
        }
        public static string ExpandObject(this JsonElement self, Dictionary<string, object> context, PolicyFunction function)
        {
            using MemoryStream ms = new MemoryStream();
            using Utf8JsonWriter writer = new Utf8JsonWriter(ms);
            writer.WriteStartObject();
            foreach (var item in self.EnumerateObject())
            {
                writer.WriteProperty(item, context, function);
            }
            writer.WriteEndObject();
            writer.Flush();
            return Encoding.UTF8.GetString(ms.ToArray());
        }

    }
}
