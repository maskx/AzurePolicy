using maskx.ARMOrchestration.Functions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace maskx.AzurePolicy.Extensions
{
    public static class JTokenExtemsions
    {
        public static JToken GetOrCreateToken(this JObject jobj, string path)
        {
            var p = path.Split('.');
            JObject pathObj = jobj;
            if (p.Length > 2)
            {
                for (int i = 0; i < p.Length - 1; i++)
                {
                    if (pathObj.TryGetValue(p[i], out JToken token))
                    {
                        pathObj = token as JObject;
                    }
                    else
                    {
                        var e = new JObject();
                        pathObj.Add(p[i], e);
                        pathObj = e;
                    }
                }
            }
            JToken rtv = jobj;
            string name = p[^1];
            bool isArray = false;
            if (name.EndsWith("[*]"))
            {
                isArray = true;
                name = name[0..^3];
            }
            if (pathObj.TryGetValue(name, out JToken t))
            {
                rtv = t;
            }
            else
            {
                if (isArray)
                    rtv = new JArray();
                else
                    rtv = new JObject();
                pathObj.Add(name, rtv);
            }
            return rtv;
        }

        public static void RemoveToken(this JObject jobj, string[] path)
        {
            var pathObj = jobj;
            string name = string.Empty;
            for (int i = 0; i < path.Length; i++)
            {
                name = path[i];
                if (name.EndsWith("]"))
                {
                    if (name.EndsWith("[*]"))
                    {
                        name = name[0..^3];
                        var p = pathObj.Property(name);
                        if (p == null)
                            return;
                        if (i == path.Length - 1)
                        {
                            p.Remove();
                            return;
                        }
                        if (!(p.Value is JArray pathArray))
                            return;
                        foreach (var item in pathArray)
                        {
                            (item as JObject).RemoveToken(path.Skip(i + 1).ToArray());
                        }
                    }
                    else
                    {
                        int index = name.IndexOf('[');
                        name = name.Substring(0, index);
                        index++;
                        var value = path[i][index..^1];
                        if (value.StartsWith('\''))
                        {
                            value = value.Trim('\'');
                        }
                        else if (value.StartsWith('\"'))
                        {
                            value = value.Trim('\"');
                        }
                        if (!(pathObj.Property(name).Value is JArray pathArray))
                            return;

                        if (i == path.Length - 1)
                        {
                            foreach (var item in pathArray)
                            {
                                if (item.ToString() == value)
                                {
                                    item.Remove();
                                    return;
                                }
                            }
                        }
                        foreach (var item in pathArray)
                        {
                            if (item.ToString() == value)
                            {
                                item.Remove();
                            }
                        }
                    }
                    return;
                }
                else
                {
                    var p = pathObj.Property(name);
                    if (p == null)
                        return;
                    if (i == path.Length - 1)
                    {
                        p.Remove();
                        return;
                    }
                    else
                    {
                        pathObj = p.Value as JObject;
                        if (pathObj == null)
                            return;
                    }
                }
            }
        }

        public static void AddOrRepleace(this JObject jobj, string[] path, JToken value)
        {
            var pathObj = jobj;
            string name = string.Empty;
            for (int i = 0; i < path.Length; i++)
            {
                name = path[i];
                if (name.EndsWith("[*]"))
                {
                    name = name[0..^3];
                    var p = pathObj.Property(name);
                    if (p == null)
                    {
                        p = new JProperty(name);
                        pathObj.Add(p);
                    }
                    if (i == path.Length - 1)
                    {
                        if (p.Value is JArray child)
                        {
                            child.Add(value);
                        }
                        else
                            p.Value = new JArray(value);
                        return;
                    }
                    if (!(p.Value is JArray pathArray))
                    {
                        pathArray = new JArray();
                        p.Value = pathArray;
                    }
                    foreach (var item in pathArray)
                    {
                        (item as JObject).AddOrRepleace(path.Skip(i + 1).ToArray(), value);
                    }
                    return;
                }
                else
                {
                    var p = pathObj.Property(name);
                    if (p == null)
                    {
                        p = new JProperty(name);
                        pathObj.Add(p);
                    }
                    if (i == path.Length - 1)
                    {
                        p.Value = value;
                        return;
                    }
                    else
                    {
                        pathObj = p.Value as JObject;
                        if (pathObj == null)
                        {
                            pathObj = new JObject();
                            p.Value = pathObj;
                        }
                    }
                }
            }
        }

        public static void Add(this JObject jobj, string[] path, JToken value)
        {
            var pathObj = jobj;
            string name = string.Empty;
            for (int i = 0; i < path.Length; i++)
            {
                name = path[i];
                if (name.EndsWith("[*]"))
                {
                    name = name[0..^3];
                    var p = pathObj.Property(name);
                    if (p == null)
                    {
                        p = new JProperty(name);
                        pathObj.Add(p);
                    }
                    if (i == path.Length - 1)
                    {
                        if (p.Value is JArray child)
                            child.Add(value);
                        else
                            p.Value = new JArray(value);
                        return;
                    }
                    if (!(p.Value is JArray pathArray))
                    {
                        pathArray = new JArray();
                        p.Value = pathArray;
                    }
                    foreach (var item in pathArray)
                    {
                        (item as JObject).Add(path.Skip(i + 1).ToArray(), value);
                    }
                    return;
                }
                else
                {
                    var p = pathObj.Property(name);
                    if (p == null)
                    {
                        p = new JProperty(name);
                        pathObj.Add(p);
                    }
                    else if (i == path.Length - 1) // only create new, not modify existing node
                        return;
                    if (i == path.Length - 1)
                    {
                        p.Value = value;
                        return;
                    }
                    else
                    {
                        pathObj = p.Value as JObject;
                        if (pathObj == null)
                        {
                            pathObj = new JObject();
                            p.Value = pathObj;
                        }
                    }
                }
            }
        }
        private static (string First, string Remain) SplitPath(string path)
        {
            int index = path.IndexOf("[*]");
            if (index < 0)
                return (path, "");
            else if (index == 0)
                return ("[*]", path.Substring(3));
            else
                return (path.Substring(0, index), path.Substring(index));
        }
        public static List<object> GetElements(this JToken token, List<string> path)
        {
            List<object> rtv = new List<object>();
            var p = SplitPath(path[0]);
            var paths=new List<string>();
            if (!string.IsNullOrEmpty(p.Remain))
                paths.Add(p.Remain);
            for (int i = 1; i < path.Count; i++)
            {
                paths.Add(path[i]);
            }
            if (token is JArray array)
            {
                if (p.First == "[*]")
                {
                    foreach (var a in array)
                    {
                        if (paths.Count == 0)
                            rtv.Add(a.GetValue());
                        else
                            rtv.AddRange(a.GetElements(paths));
                    }
                }
            }
            else if (token is JObject obj)
            {
                if (obj.TryGetValue(p.First, out JToken t))
                {
                    if (paths.Count == 0)
                        rtv.Add(t.GetValue());
                    else
                        rtv.AddRange(t.GetElements(paths));
                }
            }
            return rtv;
        }
        public static object GetValue(this JToken token)
        {
            object rtv = null;
            switch (token.Type)
            {
                case JTokenType.None:
                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                case JTokenType.Date:
                    break;
                case JTokenType.Object:
                case JTokenType.Array:
                    rtv = new JsonValue(token.ToString(Newtonsoft.Json.Formatting.Indented));
                    break;
                case JTokenType.Float:
                case JTokenType.Integer:
                    rtv = token.Value<int>();
                    break;
                case JTokenType.String:
                    rtv = token.Value<string>();
                    break;
                case JTokenType.Boolean:
                    rtv = token.Value<bool>();
                    break;
                default:
                    break;
            }
            return rtv;
        }
    }
}
