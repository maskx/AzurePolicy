using maskx.AzurePolicy.Extensions;
using maskx.AzurePolicy.Functions;
using maskx.ARMOrchestration.Functions;
using maskx.ARMOrchestration.Orchestrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

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
            this._Conditions.Add("notEquals", (left, right) =>
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
            this._Conditions.Add("notLike", (left, right) =>
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
                return ConditionMatch(left.ToString(), right.ToString(), false);
            });
            this._Conditions.Add("matchInsensitively", (left, right) =>
            {
                return ConditionMatch(left.ToString(), right.ToString(), true);
            });
            this._Conditions.Add("notMatch", (left, right) =>
            {
                return !ConditionMatch(left.ToString(), right.ToString(), false);
            });
            this._Conditions.Add("notMatchInsensitively", (left, right) =>
            {
                return !ConditionMatch(left.ToString(), right.ToString(), true);
            });
            this._Conditions.Add("contains", (left, right) =>
            {
                var list = (left as List<object>).Select(s => (string)s).ToList();
                return (list.Contains(right.ToString()));
            });
            this._Conditions.Add("notContains", (left, right) =>
            {
                var list = (left as List<object>).Select(s => (string)s).ToList();
                return !(list.Contains(right.ToString()));

            });
            this._Conditions.Add("in", (left, right) =>
            {
                return (right as JsonValue).Contains(left);
            });
            this._Conditions.Add("notIn", (left, right) =>
            {
                return !(right as JsonValue).Contains(left);
            });
            this._Conditions.Add("containsKey", (left, right) =>
            {
                var s = left.ToString().Split('*');
                var r = right.ToString();
                // TODO: containsKey
                return !string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            });
            this._Conditions.Add("notContainsKey", (left, right) =>
            {
                var s = left.ToString().Split('*');
                var r = right.ToString();
                // TODO: notContainsKey
                return !string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
            });
            this._Conditions.Add("less", (left, right) =>
            {
                return ConditionCompare(left.ToString(), right.ToString(), "less");
            });
            this._Conditions.Add("lessOrEquals", (left, right) =>
            {
                return ConditionCompare(left.ToString(), right.ToString(), "lessOrEquals");
            });
            this._Conditions.Add("greater", (left, right) =>
            {
                return ConditionCompare(left.ToString(), right.ToString(), "greater");
            });
            this._Conditions.Add("greaterOrEquals", (left, right) =>
            {
                return ConditionCompare(left.ToString(), right.ToString(), "greaterOrEquals");
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
                        left = _PolicyFunction.Field(path, policyCxt.Resource, deployCxt, policyCxt.NamePath);
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
                //TODO:confirm condition name spellformat
                else if (_Conditions.TryGetValue(item.Name, out func))//else if (_Conditions.TryGetValue(item.Name.ToLower(), out func))
                {
                    right = item.Value.GetEvaluatedValue(_ARMFunctions, context);
                }
            }
            if (func == null)
                throw new Exception("cannot find conditions");
            return func(left, right);
        }
       

        public int Count(JsonElement element, Dictionary<string, object> context, string namePath = "")
        {
            if (!element.TryGetProperty("field", out JsonElement fieldE))
                return -1;
            var deployCxt = context[Functions.ContextKeys.DEPLOY_CONTEXT] as DeploymentContext;
            var policyCxt = context[Functions.ContextKeys.POLICY_CONTEXT] as PolicyContext;
            var path = this._PolicyFunction.Evaluate(fieldE.GetString(), context).ToString();
            if (!(_PolicyFunction.Field(path, policyCxt.Resource, deployCxt, namePath) is List<object> d))
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

        private bool ConditionMatch(string left,string right,bool insensitively)
        {
            if (left.Length != right.Length)
                return false;
            int matched = 0;
            for(int i=0;i<right.Length;)
            {
                if (matched > left.Length)
                    return false;
                char c = right[i++];
                if (c == '.') // Any single character
                {
                    matched++;
                }
                else if (c == '#') // Any single digit
                {
                    if (!Char.IsDigit(left[matched]))
                        return false;
                    matched++;
                }
                else if (c == '?') // letter
                {
                    if (!Char.IsLetter(left[matched]))
                        return false;
                    matched++;
                }
                else // Exact character
                {
                    if (insensitively)
                    {
                        if (c != left[matched])
                            return false;
                    }
                    else
                    {
                        if (Char.ToLower(c) != Char.ToLower(left[matched]))
                            return false;
                    }
                    matched++;
                }
            }
            return (matched == left.Length);


        }

        private bool ConditionCompare(string left,string right,string operation)
        {
            bool leftParse = false;
            bool rightParse = false;
            DateTime leftTime = new DateTime();
            DateTime rightTime = new DateTime();

            leftParse=DateTime.TryParse(left, out leftTime);
            rightParse = DateTime.TryParse(right, out rightTime);
            if(((leftParse==false)&&(rightParse==true))|| ((leftParse==true) && (rightParse==false)))
            {
                throw new Exception($"field and input of condition can not both parse to datetime,field:{left},input:{right}");
            }
            //only both are datetime
            if (leftParse && rightParse)
            {
                switch (operation)
                {
                    case "less":
                        return leftTime < rightTime;
                    case "lessOrEquals":
                        return leftTime <= rightTime;
                    case "greater":
                        return leftTime > rightTime;
                    case "greaterOrEquals":
                        return leftTime >= rightTime;
                    default:
                        throw new NotImplementedException();
                }
            }
             leftParse = false;
             rightParse = false;
            Int32 leftInt = new Int32();
            Int32 rightInt = new Int32();

            leftParse = Int32.TryParse(left, out leftInt);
            rightParse = Int32.TryParse(right, out rightInt);
            if (((!leftParse) && rightParse) || (leftParse && (!rightParse)))
            {
                throw new Exception($"field and input of condition can not both parse to int,field:{left},input:{right}");
            }
            //only both are int
            if (leftParse && rightParse)
            {
                switch (operation)
                {
                    case "less":
                        return leftInt < rightInt;
                    case "lessOrEquals":
                        return leftInt <= rightInt;
                    case "greater":
                        return leftInt > rightInt;
                    case "greaterOrEquals":
                        return leftInt >= rightInt;
                    default:
                        throw new NotImplementedException();

                }
            }
            //end with string compare
            switch (operation)
            {
                case "less":
                    return (string.Compare(left,right)<0);
                case "lessOrEquals":
                    return (string.Compare(left, right) <=0);
                case "greater":
                    return (string.Compare(left, right) > 0);
                case "greaterOrEquals":
                    return (string.Compare(left, right) >= 0);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
