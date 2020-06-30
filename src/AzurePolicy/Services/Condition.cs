using maskx.AzurePolicy.Extensions;
using maskx.AzurePolicy.Functions;
using maskx.ARMOrchestration.Functions;
using maskx.ARMOrchestration.Orchestrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace maskx.AzurePolicy.Services
{
    public class Condition
    {
        private readonly Dictionary<string, Func<object, object, bool>> _Conditions = new Dictionary<string, Func<object, object, bool>>();
        private readonly PolicyFunction _PolicyFunction;
        private readonly ARMFunctions _ARMFunctions;
        private readonly IServiceProvider _ServiceProvider;
        public Condition(IServiceProvider serviceProvider, PolicyFunction function, ARMFunctions aRMFunctions)
        {
            this._PolicyFunction = function;
            this._ARMFunctions = aRMFunctions;
            this._ServiceProvider = serviceProvider;
            InitBuiltInCodition();
        }
        private void InitBuiltInCodition()
        {
            this._Conditions.Add("equals", (left, right) =>
            {
                return string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            });
            this._Conditions.Add("notequals", (left, right) =>
            {
                return !string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            });
            // When using the like and notLike conditions, you provide a wildcard * in the value. The value shouldn't have more than one wildcard *.
            this._Conditions.Add("like", (left, right) =>
            {
                if (right.ToString().Equals("*"))
                {
                    return true;
                }
                var s = right.ToString().Split('*');
                var r = left.ToString();
                if (s.Length < 2)
                {
                    throw new Exception("value field not contains *");
                }
                else if (s.Length > 2)
                {
                    throw new Exception("value field only contains one *");
                }
                if (string.IsNullOrEmpty(s[0]))
                {
                    return (r.EndsWith(s[1], StringComparison.OrdinalIgnoreCase));
                }
                else if (string.IsNullOrEmpty(s[1]))
                {
                    return r.StartsWith(s[0], StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    return (r.StartsWith(s[0], StringComparison.OrdinalIgnoreCase) && r.EndsWith(s[1], StringComparison.OrdinalIgnoreCase));
                }
            });
            this._Conditions.Add("notlike", (left, right) =>
            {
                if (right.ToString().Equals("*"))
                {
                    return false;
                }
                var s = right.ToString().Split('*');
                var r = left.ToString();
                if (s.Length < 2)
                {
                    throw new Exception("value field not contains *");
                }
                else if (s.Length > 2)
                {
                    throw new Exception("value field only contains one *");
                }
                if (string.IsNullOrEmpty(s[0]))
                {
                    return (!r.EndsWith(s[1], StringComparison.OrdinalIgnoreCase));
                }
                else if (string.IsNullOrEmpty(s[1]))
                {
                    return !r.StartsWith(s[0], StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    return ((!r.StartsWith(s[0], StringComparison.OrdinalIgnoreCase)) || (!r.EndsWith(s[1], StringComparison.OrdinalIgnoreCase)));
                }
            });
            // When using the match and notMatch conditions, provide # to match a digit, ? for a letter, . to match any character, and any other character to match that actual character. While match and notMatch are case-sensitive, all other conditions that evaluate a stringValue are case-insensitive. Case-insensitive alternatives are available in matchInsensitively and notMatchInsensitively.
            this._Conditions.Add("match", (left, right) =>
            {
                var s = left.ToString().Split('*');
                var r = right.ToString();
                // TODO: match
                return !string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            });
            this._Conditions.Add("matchinsensitively", (left, right) =>
            {
                var s = left.ToString().Split('*');
                var r = right.ToString();
                // TODO: matchInsensitively
                return !string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            });
            this._Conditions.Add("notmatch", (left, right) =>
            {
                var s = left.ToString().Split('*');
                var r = right.ToString();
                // TODO: notMatch
                return !string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            });
            this._Conditions.Add("notmatchinsensitively", (left, right) =>
            {
                var s = left.ToString().Split('*');
                var r = right.ToString();
                // TODO: notMatchInsensitively
                return !string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            });
            this._Conditions.Add("contains", (left, right) =>
            {
                var list = (left as List<object>).Select(s => (string)s).ToList();
                return (list.Contains(right.ToString()));
            });
            this._Conditions.Add("notcontains", (left, right) =>
            {
                var list = (left as List<object>).Select(s => (string)s).ToList();
                return !(list.Contains(right.ToString()));

            });
            this._Conditions.Add("in", (left, right) =>
            {
                return (right as JsonValue).Contains(left);
            });
            this._Conditions.Add("notin", (left, right) =>
            {
                return !(right as JsonValue).Contains(left);
            });
            this._Conditions.Add("containskey", (left, right) =>
            {
                var s = left.ToString().Split('*');
                var r = right.ToString();
                // TODO: containsKey
                return !string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            });
            this._Conditions.Add("notcontainskey", (left, right) =>
            {
                var s = left.ToString().Split('*');
                var r = right.ToString();
                // TODO: notContainsKey
                return !string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            });
            this._Conditions.Add("less", (left, right) =>
            {
                var s = left.ToString().Split('*');
                var r = right.ToString();
                // TODO: less
                return !string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            });
            this._Conditions.Add("lessorequals", (left, right) =>
            {
                var s = left.ToString().Split('*');
                var r = right.ToString();
                // TODO: lessOrEquals
                return !string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            });
            this._Conditions.Add("greater", (left, right) =>
            {
                var s = left.ToString().Split('*');
                var r = right.ToString();
                // TODO: greater
                return !string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            });
            this._Conditions.Add("greaterorequals", (left, right) =>
            {
                var s = left.ToString().Split('*');
                var r = right.ToString();
                // TODO: greaterOrEquals
                return !string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            });
            this._Conditions.Add("exists", (left, right) =>
            {
                var s = left.ToString().Split('*');
                var r = right.ToString();
                // TODO: exists
                return !string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            });
        }
        public bool Evaluate(JsonElement element, Dictionary<string, object> context)
        {
            #region note
            //left is field calculated result;right is the value of input for condition. eg: 
            /*
             *  "policyRule": {
                   "if": {
                              "field": "Microsoft.Network/virtualNetworks/addressSpace.addressPrefixes[*]",
                              "notContains": "10.0.0.0/16"
                          },
                   "then": {
                              "effect": "deny"
                            }
                 }
             */
            #endregion
            object left = null;
            object right = null;
            Func<object, object, bool> func = null;
            var deployCxt = context[Functions.ContextKeys.DEPLOY_CONTEXT] as DeploymentContext;
            var policyCxt = context[Functions.ContextKeys.POLICY_CONTEXT] as PolicyContext;
            foreach (var item in element.EnumerateObject())
            {
                if ("field".Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var path = this._PolicyFunction.Evaluate(item.Value.GetString(), context).ToString();
                    if (context.TryGetValue(Functions.ContextKeys.COUNT_FIELD, out object countFiled))
                    {

                        path = path.Remove(0, countFiled.ToString().Length);
                        if(path.StartsWith('.'))
                        {
                            path = path.Remove(0, 1);
                        }
                        var paths = path.Split('.').ToList();
                        if (string.IsNullOrEmpty(paths[0]))
                        {
                            left = context[Functions.ContextKeys.COUNT_ELEMENT];
                        }
                        else
                        {
                            using var doc = JsonDocument.Parse(context[Functions.ContextKeys.COUNT_ELEMENT].ToString());
                            left = doc.RootElement.GetElements(paths,
                                _ARMFunctions,
                                new Dictionary<string, object>() {
                                { ARMOrchestration.Functions.ContextKeys.ARM_CONTEXT, deployCxt }
                                });
                            if (!path.Contains("[*]"))
                                left = (left as List<object>).First();
                        }
                    }
                    else
                    {
                        left = Field(path, policyCxt.Resource, deployCxt, policyCxt.NamePath);
                    }
                }
                else if ("value".Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    left = this._PolicyFunction.Evaluate(item.Value.GetString(), context);
                }
                else if ("count".Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var r = Count(item.Value, context, policyCxt.NamePath);
                    if (r == -1)// https://docs.microsoft.com/en-us/azure/governance/policy/concepts/definition-structure#count
                        return false;
                    else
                        left = r;
                }
                else if (_Conditions.TryGetValue(item.Name.ToLower(), out func))
                {
                    right = item.Value.GetEvaluatedValue(_ARMFunctions, context);
                }
            }
            if (func == null)
                throw new Exception("cannot find conditions");
            return func(left, right);
        }
        public object Field(string fieldPath, string resource, DeploymentContext deployDontext, string namePath = "")
        {
            using var doc = JsonDocument.Parse(resource);
            var root = doc.RootElement;
            var context = new Dictionary<string, object>() { { ARMOrchestration.Functions.ContextKeys.ARM_CONTEXT, deployDontext } };

            if (fieldPath.Contains('/'))//property aliases
            {
                string fullType = GetFullType(deployDontext, namePath, root);
                if (!fieldPath.StartsWith(fullType))
                    return -1;
                var p = fieldPath.Remove(0, fullType.Length + 1);
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

        public int Count(JsonElement element, Dictionary<string, object> context, string namePath = "")
        {
            if (!element.TryGetProperty("field", out JsonElement fieldE))
                return -1;
            var deployCxt = context[Functions.ContextKeys.DEPLOY_CONTEXT] as DeploymentContext;
            var policyCxt = context[Functions.ContextKeys.POLICY_CONTEXT] as PolicyContext;
            var path = this._PolicyFunction.Evaluate(fieldE.GetString(), context).ToString();
            if (!(Field(path, policyCxt.Resource, deployCxt, namePath) is List<object> d))
                return 0;
            if (element.TryGetProperty("where", out JsonElement whereE))
            {
                var logical = this._ServiceProvider.GetService<Logical>();
                return d.Where(e =>
                {
                    return logical.Evaluate(whereE, new Dictionary<string, object>() {
                        {Functions.ContextKeys.DEPLOY_CONTEXT,deployCxt },
                        {Functions.ContextKeys.POLICY_CONTEXT,policyCxt },
                        {Functions.ContextKeys.COUNT_ELEMENT,e },
                        {Functions.ContextKeys.COUNT_FIELD,path }
                    });

                }).Count();
            }

            return d.Count();
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

        private bool Match(string left,string right)
        {
            return false;
        }
    }
}
