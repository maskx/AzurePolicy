using maskx.ARMOrchestration.Functions;
using maskx.ARMOrchestration.Orchestrations;
using maskx.AzurePolicy.Extensions;
using maskx.AzurePolicy.Services;
using maskx.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace maskx.AzurePolicy.Functions
{
    /// <summary>
    /// <see cref="https://docs.microsoft.com/en-us/azure/governance/policy/concepts/definition-structure#policy-functions"/>
    /// </summary>
    public class PolicyFunction
    {
        private readonly Dictionary<string, Action<FunctionArgs, Dictionary<string, object>>> Functions = new Dictionary<string, Action<FunctionArgs, Dictionary<string, object>>>();
        private readonly ARMFunctions _ARMFunctions;
        public PolicyFunction(ARMFunctions aRMFunctions)
        {
            this._ARMFunctions = aRMFunctions;
            InitBuiltInFunctions();
        }
        private void InitBuiltInFunctions()
        {
            #region Array and object

            Functions.Add("array", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                string str = string.Empty;
                if (par1 is string)
                {
                    str = $"[\"{par1}\"]";
                }
                else if (par1 is bool f)
                {
                    str = f ? "true" : "false";
                }
                else if (par1 is JsonValue)
                {
                    str = $"[{(par1 as JsonValue).RawString}]";
                }
                else
                    str = $"[{par1}]";
                args.Result = new JsonValue(str);
            });
            Functions.Add("coalesce", (args, cxt) =>
            {
                for (int i = 0; i < args.Parameters.Length; i++)
                {
                    var a = args.Parameters[i].Evaluate(cxt);
                    if (a != null)
                    {
                        args.Result = a;
                        break;
                    }
                }
            });
            Functions.Add("concat", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                if (pars[0] is JsonValue)
                    args.Result = new JsonValue($"[{string.Join(",", pars)}]");
                else
                    args.Result = string.Join("", pars);
            });
            Functions.Add("contains", (args, cxt) =>
            {
                args.Result = false;
                var pars = args.EvaluateParameters(cxt);
                if (pars[0] is string s)
                {
                    if (s.IndexOf(pars[1] as string) > 0)
                        args.Result = true;
                }
                else if (pars[0] is JsonValue jv)
                {
                    args.Result = jv.Contains(pars[1]);
                }
            });
            Functions.Add("createarray", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = new JsonValue($"[{string.Join(",", pars.Select(JsonValue.PackageJson))}]");
            });
            Functions.Add("first", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                if (par1 is string s)
                    args.Result = s[0].ToString();
                else if (par1 is JsonValue jv)
                    args.Result = jv[0];
            });
            Functions.Add("intersection", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                JsonValue jv = pars[0] as JsonValue;
                for (int i = 1; i < pars.Length; i++)
                {
                    jv = jv.Intersect(pars[i] as JsonValue);
                }
                args.Result = jv;
            });
            Functions.Add("json", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                args.Result = new JsonValue(par1.ToString());
            });
            Functions.Add("last", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                if (par1 is string s)
                    args.Result = s.Last().ToString();
                else if (par1 is JsonValue jv)
                    args.Result = jv[^1];
            });
            Functions.Add("length", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                if (par1 is string s)
                    args.Result = s.Length;
                else if (par1 is JsonValue jv)
                    args.Result = jv.Length;
                else if (par1 is List<object> l)
                    args.Result = l.Count;
            });
            Functions.Add("max", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                if (pars[0] is JsonValue jv)
                {
                    args.Result = jv.Max();
                }
                else
                {
                    args.Result = pars.Max();
                }
            });
            Functions.Add("min", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                if (pars[0] is JsonValue jv)
                {
                    args.Result = jv.Min();
                }
                else
                {
                    args.Result = pars.Min();
                }
            });
            Functions.Add("range", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                List<int> nums = new List<int>();
                int index = Convert.ToInt32(pars[0]);
                int length = index + Convert.ToInt32(pars[1]);
                for (; index < length; index++)
                {
                    nums.Add(index);
                }
                args.Result = new JsonValue($"[{string.Join(",", nums)}]");
            });
            Functions.Add("skip", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                int numberToSkip = Convert.ToInt32(pars[1]);
                if (pars[0] is string s)
                {
                    args.Result = s.Substring(numberToSkip);
                }
                else if (pars[0] is JsonValue jv)
                {
                    List<string> ele = new List<string>();
                    for (int i = numberToSkip; i < jv.Length; i++)
                    {
                        ele.Add(JsonValue.PackageJson(jv[i]));
                    }
                    args.Result = new JsonValue($"[{string.Join(",", ele)}]");
                }
            });
            Functions.Add("take", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                int numberToTake = Convert.ToInt32(pars[1]);
                if (pars[0] is string s)
                {
                    args.Result = s.Substring(0, numberToTake);
                }
                else if (pars[0] is JsonValue jv)
                {
                    List<string> ele = new List<string>();
                    for (int i = 0; i < numberToTake; i++)
                    {
                        ele.Add(JsonValue.PackageJson(jv[i]));
                    }
                    args.Result = new JsonValue($"[{string.Join(",", ele)}]");
                }
            });
            Functions.Add("union", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                JsonValue jv = pars[0] as JsonValue;
                for (int i = 1; i < pars.Length; i++)
                {
                    jv = jv.Union(pars[i] as JsonValue);
                }
                args.Result = jv;
            });

            #endregion Array and object

            #region Comparison

            Functions.Add("equals", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                if (pars[0] is JsonValue jv)
                {
                    args.Result = jv.Equals(pars[1]);
                }
                else
                {
                    args.Result = pars[0].ToString() == pars[1].ToString();
                }
            });
            Functions.Add("greater", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = false;
                if (pars[0] is string s)
                {
                    args.Result = string.Compare(s, pars[1] as string) > 0;
                }
                else
                {
                    //ARM only support int
                    args.Result = Convert.ToInt32(pars[0]) > Convert.ToInt32(pars[1]);
                }
            });
            Functions.Add("greaterorequals", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = false;
                if (pars[0] is string s)
                {
                    args.Result = string.Compare(s, pars[1] as string) >= 0;
                }
                else
                {
                    //ARM only support int
                    args.Result = Convert.ToInt32(pars[0]) >= Convert.ToInt32(pars[1]);
                }
            });
            Functions.Add("less", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = false;
                if (pars[0] is string s)
                {
                    args.Result = string.Compare(s, pars[1] as string) < 0;
                }
                else
                {
                    //ARM only support int
                    args.Result = Convert.ToInt32(pars[0]) < Convert.ToInt32(pars[1]);
                }
            });
            Functions.Add("lessorequals", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = false;
                if (pars[0] is string s)
                {
                    args.Result = string.Compare(s, pars[1] as string) <= 0;
                }
                else
                {
                    //ARM only support int
                    args.Result = Convert.ToInt32(pars[0]) <= Convert.ToInt32(pars[1]);
                }
            });

            #endregion Comparison

            #region Logical

            Functions.Add("and", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = true;
                foreach (var item in pars)
                {
                    if (!(bool)item)
                    {
                        args.Result = false;
                        break;
                    }
                }
            });
            Functions.Add("bool", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt).ToString().ToLower();
                if (par1 == "1" || par1 == "true")
                    args.Result = true;
                else
                    args.Result = false;
            });
            Functions.Add("if", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                if ((bool)pars[0])
                    args.Result = pars[1];
                else
                    args.Result = pars[2];
            });
            Functions.Add("not", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                args.Result = !(bool)par1;
            });
            Functions.Add("or", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = false;
                foreach (var item in pars)
                {
                    if ((bool)item)
                    {
                        args.Result = true;
                        break;
                    }
                }
            });

            #endregion Logical

            #region String

            Functions.Add("base64", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                var plainTextBytes = Encoding.UTF8.GetBytes(par1 as string);
                args.Result = Convert.ToBase64String(plainTextBytes);
            });
            Functions.Add("base64tostring", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                var base64EncodedBytes = Convert.FromBase64String(par1 as string);
                args.Result = Encoding.UTF8.GetString(base64EncodedBytes);
            });
            Functions.Add("base64tojson", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                var base64EncodedBytes = Convert.FromBase64String(par1 as string);
                args.Result = new JsonValue(Encoding.UTF8.GetString(base64EncodedBytes));
            });
            Functions.Add("datauri", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                var plainTextBytes = Encoding.UTF8.GetBytes(par1 as string);
                args.Result = "data:text/plain;charset=utf8;base64," + Convert.ToBase64String(plainTextBytes);
            });
            Functions.Add("datauritostring", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                var s = (par1 as string);
                s = s.Substring(s.LastIndexOf(',') + 1).Trim();
                var base64EncodedBytes = Convert.FromBase64String(s);
                args.Result = Encoding.UTF8.GetString(base64EncodedBytes);
            });
            Functions.Add("endswith", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = (pars[0] as string).EndsWith(pars[1] as string, StringComparison.InvariantCultureIgnoreCase);
            });
            Functions.Add("startswith", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = (pars[0] as string).StartsWith(pars[1] as string, StringComparison.InvariantCultureIgnoreCase);
            });
            Functions.Add("format", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = string.Format((pars[0] as string), pars.Skip(1).ToArray());
            });
            Functions.Add("guid", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] bytes = md5.ComputeHash(Encoding.Unicode.GetBytes(string.Join('-', pars)));
                args.Result = new Guid(bytes).ToString();
            });
            // https://stackoverflow.com/a/48305669
            Functions.Add("uniquestring", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                string result = "";
                var buffer = Encoding.UTF8.GetBytes(string.Join('-', pars));
                var hashArray = new SHA512Managed().ComputeHash(buffer);
                for (int i = 1; i <= 13; i++)
                {
                    var b = hashArray[i];
                    if (b >= 48 && b <= 57)// keep number
                        result += Convert.ToChar(b);
                    else // change to letter
                        result += Convert.ToChar((b % 26) + (byte)'a');
                }
                args.Result = result;
            });
            Functions.Add("indexof", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = (pars[0] as string).IndexOf(pars[1] as string, StringComparison.InvariantCultureIgnoreCase);
            });
            Functions.Add("lastindexof", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = (pars[0] as string).LastIndexOf(pars[1] as string, StringComparison.InvariantCultureIgnoreCase);
            });
            Functions.Add("padleft", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                var s = pars[0] as string;
                var width = (int)pars[1];
                if (pars.Length > 2)
                {
                    char c = pars[2].ToString()[0];
                    args.Result = s.PadLeft(width, c);
                }
                else
                    args.Result = s.PadLeft(width);
            });
            Functions.Add("replace", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = (pars[0] as string).Replace(pars[1] as string, pars[2] as string);
            });
            Functions.Add("split", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                var str = pars[0] as string;
                string[] rtv;
                if (pars[1] is string split)
                    rtv = str.Split(split[0]);
                else if (pars[1] is JsonValue jv)
                {
                    var splits = new char[jv.Length];
                    for (int i = 0; i < splits.Length; i++)
                    {
                        splits[i] = (jv[i] as string)[0];
                    }
                    rtv = str.Split(splits);
                }
                else
                    rtv = null;
                args.Result = new JsonValue($"[\"{ string.Join("\",\"", rtv)}\"]");
            });

            Functions.Add("newguid", (args, cxt) =>
            {
                args.Result = Guid.NewGuid().ToString();
            });
            Functions.Add("string", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                args.Result = par1.ToString();
            });
            Functions.Add("empty", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                args.Result = false;
                if (par1 is null)
                    args.Result = true;
                else if (par1 is JsonValue)
                {
                    var p = par1 as JsonValue;
                    if (p.RawString == "{}" || p.RawString == "[]" || p.RawString == "null")
                        args.Result = true;
                }
                else if (par1 is string && string.IsNullOrEmpty(par1 as string))
                    args.Result = true;
            });
            Functions.Add("substring", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                var s = pars[0] as string;
                var startIndex = (int)pars[1];
                if (pars.Length > 2)
                {
                    args.Result = s.Substring(startIndex, (int)pars[2]);
                }
                else
                {
                    args.Result = s.Substring(startIndex);
                }
            });
            Functions.Add("tolower", (args, cxt) =>
            {
                args.Result = args.Parameters[0].Evaluate(cxt).ToString().ToLower();
            });
            Functions.Add("toupper", (args, cxt) =>
            {
                args.Result = args.Parameters[0].Evaluate(cxt).ToString().ToUpper();
            });
            Functions.Add("trim", (args, cxt) =>
            {
                args.Result = args.Parameters[0].Evaluate(cxt).ToString().Trim();
            });
            Functions.Add("uri", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = System.IO.Path.Combine(pars[0] as string, pars[1] as string);
            });
            Functions.Add("uricomponent", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                args.Result = Uri.EscapeDataString(par1 as string);
            });
            Functions.Add("uricomponenttostring", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                args.Result = Uri.UnescapeDataString(par1 as string);
            });
            Functions.Add("utcnow", (args, cxt) =>
            {
                if (args.Parameters.Length > 0)
                {
                    args.Result = DateTime.UtcNow.ToString(args.Parameters[0].Evaluate(cxt).ToString());
                }
                else
                {
                    args.Result = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");
                }
            });

            #endregion String

            #region Numeric

            Functions.Add("add", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = Convert.ToInt32(pars[0]) + Convert.ToInt32(pars[1]);
            });

            Functions.Add("div", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = Convert.ToInt32(pars[0]) / Convert.ToInt32(pars[1]);
            });
            Functions.Add("float", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                args.Result = Convert.ToDecimal(par1);
            });
            Functions.Add("int", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt);
                args.Result = Convert.ToInt32(par1);
            });
            Functions.Add("mod", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                var d = Math.DivRem(Convert.ToInt32(pars[0]), Convert.ToInt32(pars[1]), out int result);
                args.Result = result;
            });
            Functions.Add("mul", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = Convert.ToInt32(pars[0]) * Convert.ToInt32(pars[1]);
            });
            Functions.Add("sub", (args, cxt) =>
            {
                var pars = args.EvaluateParameters(cxt);
                args.Result = Convert.ToInt32(pars[0]) - Convert.ToInt32(pars[1]);
            });

            #endregion Numeric

            #region Policy
            Functions.Add("parameters", (args, cxt) =>
            {
                var par1 = args.Parameters[0].Evaluate(cxt).ToString();
                if (cxt.TryGetValue(ContextKeys.INITIATIVE_PARAMERTERS, out object initiative))
                {
                    using var jsonDoc = JsonDocument.Parse(initiative.ToString());
                    if (jsonDoc.RootElement.TryGetProperty(par1, out JsonElement ele))
                    {
                        if (ele.TryGetProperty("value", out JsonElement v))
                        {
                            args.Result = JsonValue.GetElementValue(v);
                        }
                    }
                }
                else if (cxt.TryGetValue(ContextKeys.POLICY_CONTEXT, out object policyContext))
                {
                    var policyCxt = policyContext as PolicyContext;
                    if (!string.IsNullOrEmpty(policyCxt.Parameters))
                    {
                        using var jsonDoc = JsonDocument.Parse(policyCxt.Parameters);
                        if (!jsonDoc.RootElement.TryGetProperty(par1, out JsonElement ele))
                        {
                            throw new Exception($"ARM Template does not define the parameter:{par1}");
                        }
                        if (ele.TryGetProperty("defaultValue", out JsonElement defValue))
                        {
                            args.Result = JsonValue.GetElementValue(defValue);
                        }
                    }
                    else
                    {
                        var pds = policyCxt.PolicyDefinition.Parameters;
                        using var defineDoc = JsonDocument.Parse(pds);
                        if (!defineDoc.RootElement.TryGetProperty(par1, out JsonElement parEleDef))
                        {
                            throw new Exception($"ARM Template does not define the parameter:{par1}");
                        }
                        if (parEleDef.TryGetProperty("defaultValue", out JsonElement defValue))
                        {
                            args.Result = JsonValue.GetElementValue(defValue);
                        }
                    }

                }
                if (args.Result is string s)
                    args.Result = Evaluate(s, cxt);
            });
            Functions.Add("field", (args, cxt) =>
            {
                if (!cxt.TryGetValue(ContextKeys.POLICY_CONTEXT, out object poliyContext))
                {
                    throw new Exception("can not find Policy Context");
                }
                if (!cxt.TryGetValue(ContextKeys.DEPLOY_CONTEXT, out object depolyContext))
                {
                    throw new Exception("can not find Policy Context");
                }
                var policyCxt = poliyContext as PolicyContext;
                var depolyCxt = depolyContext as DeploymentContext;
                var par = args.EvaluateParameters(cxt);
                args.Result = Field(par[0].ToString(), policyCxt.Resource, depolyCxt);
            });
            #endregion
        }
        public object Evaluate(string function, Dictionary<string, object> context)
        {
            if (string.IsNullOrEmpty(function))
                return string.Empty;
            // https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/template-expressions#escape-characters
            if (function.StartsWith("[") && function.EndsWith("]") && !function.StartsWith("[["))
            {
                string functionString = function.TrimStart('[').TrimEnd(']');
                var expression = new Expression.Expression(functionString)
                {
                    EvaluateFunction = (name, args, cxt) =>
                    {
                        if (Functions.TryGetValue(name.ToLower(), out Action<FunctionArgs, Dictionary<string, object>> func))
                        {
                            func(args, cxt);
                        }
                    }
                };
                return expression.Evaluate(context);
            }
            return function;
        }

        public object Field(string fieldPath, string resource, DeploymentContext deployDontext, string namePath = "")
        {
            using var doc = JsonDocument.Parse(resource);
            var root = doc.RootElement;
            var context = new Dictionary<string, object>() { { ARMOrchestration.Functions.ContextKeys.ARM_CONTEXT, deployDontext } };
            int index = fieldPath.LastIndexOf('/');
            if (index>0)//property aliases
            {
                string fullType = GetFullType(deployDontext, namePath, root);
                string type = fieldPath.Substring(0, index);
                if (!string.Equals(fullType,type,StringComparison.OrdinalIgnoreCase))
                    return -1;
                var p = fieldPath.Remove(0,index+1);
                var r = root.GetProperty("properties").GetElements(p.Split('.').ToList(), _ARMFunctions, context);
                if (p.Contains("[*]"))
                    return r;
                return r.First();
            }
            else if ("fullName".Equals(fieldPath, StringComparison.OrdinalIgnoreCase))
            {
                if (!root.TryGetProperty("name", out JsonElement nameE))
                    throw new Exception("cannot find 'name' property");
                var name = this._ARMFunctions.Evaluate(nameE.GetString(), context).ToString();
                if (string.IsNullOrEmpty(namePath))
                    return name;
                return $"{namePath}/{name}";
            }
            else if ("type".Equals(fieldPath, StringComparison.OrdinalIgnoreCase))
            {
                return GetFullType(deployDontext, namePath, root);
            }
            var e = root.GetElementDotWithoutException(fieldPath);
            if (e.IsEqual(default))
                return null;
            return e.GetEvaluatedValue(_ARMFunctions, context);
        }

        private string GetFullType(DeploymentContext context, string namePath, JsonElement root)
        {
            if (!root.TryGetProperty("type", out JsonElement typeE))
                throw new Exception("cannot find 'type' property");
            var type = this._ARMFunctions.Evaluate(typeE.GetString(), new Dictionary<string, object>()
                {
                    {ARMOrchestration.Functions.ContextKeys.ARM_CONTEXT,context }

                }).ToString();
            if (string.IsNullOrEmpty(namePath))
                return type;
            string fulltype = GetParentType(context.TemplateContent, namePath.Split('/'), new Dictionary<string, object>()
                {
                    { ARMOrchestration.Functions.ContextKeys.ARM_CONTEXT,context }

                });
            return $"{fulltype}/{type}";
        }

        private string GetParentType(string template, string[] path, Dictionary<string, object> context)
        {
            List<string> types = new List<string>();
            using var doc = JsonDocument.Parse(template);
            JsonElement element = doc.RootElement.GetProperty("resources");
            foreach (var p in path)
            {
                foreach (var r in element.EnumerateArray())
                {
                    if (p == this._ARMFunctions.Evaluate(r.GetProperty("name").GetString(), context).ToString())
                    {
                        types.Add(this._ARMFunctions.Evaluate(r.GetProperty("type").GetString(), context).ToString());
                        element = r.GetProperty("resources");
                        break;
                    }

                }
            }
            return string.Join('/', types);

        }
    }
}
