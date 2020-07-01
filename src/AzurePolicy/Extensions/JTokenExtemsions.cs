using Newtonsoft.Json.Linq;

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
    }
}
