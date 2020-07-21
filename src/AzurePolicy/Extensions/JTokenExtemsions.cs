using Newtonsoft.Json.Linq;
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
                            (item as JObject).RemoveToken(path.Skip(i).ToArray());
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
    }
}
