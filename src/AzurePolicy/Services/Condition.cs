﻿using maskx.ARMOrchestration.Functions;
using maskx.AzurePolicy.Extensions;
using maskx.AzurePolicy.Functions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace maskx.AzurePolicy.Services
{
    public class Condition
    {
        private readonly Dictionary<string, Func<JsonElement, Dictionary<string, object>, object>> _LeftConditions = new Dictionary<string, Func<JsonElement, Dictionary<string, object>, object>>();
        private readonly Dictionary<string, Func<object, object, bool>> _RightConditions = new Dictionary<string, Func<object, object, bool>>();
        private readonly PolicyFunction _PolicyFunction;
        private readonly ARMFunctions _ARMFunctions;
        private readonly IServiceProvider _ServiceProvider;

        public Condition(IServiceProvider serviceProvider, PolicyFunction function, ARMFunctions aRMFunctions)
        {
            this._PolicyFunction = function;
            this._ARMFunctions = aRMFunctions;
            this._ServiceProvider = serviceProvider;
            InitLeftCondition();
            InitRightBuiltInCodition();
        }

        #region LeftCondition
        public void SetLeftCondition(string name, Func<JsonElement, Dictionary<string, object>, object> func)
        {
            this._LeftConditions[name] = func;
        }
        private void InitLeftCondition()
        {
            this._LeftConditions.Add("field", Left_Field);
            this._LeftConditions.Add("value", Left_Value);
            this._LeftConditions.Add("count", Left_Count);
        }
        private object Left_Field(JsonElement element, Dictionary<string, object> context)
        {
            var policyCxt = context[Functions.ContextKeys.POLICY_CONTEXT] as PolicyContext;
            var path = this._PolicyFunction.Evaluate(element.GetString(), context).ToString();
            if (context.TryGetValue(Functions.ContextKeys.COUNT_FIELD, out object countFiled))
            {
                path = path.Remove(0, countFiled.ToString().Length);
                if (path.StartsWith('.'))
                {
                    path = path.Remove(0, 1);
                }
                var paths = path.Split('.').ToList();
                if (string.IsNullOrEmpty(paths[0]))
                {
                    return context[Functions.ContextKeys.COUNT_ELEMENT];
                }
                else
                {
                    using var doc = JsonDocument.Parse(context[Functions.ContextKeys.COUNT_ELEMENT].ToString());
                    object left = doc.RootElement.GetElements(paths,
                          _ARMFunctions,
                          new Dictionary<string, object>() {
                                { ARMOrchestration.Functions.ContextKeys.ARM_CONTEXT, policyCxt.Resource.Deployment }
                          });
                    if (!path.Contains("[*]"))
                        left = (left as List<object>).First();
                    return left;
                }
            }
            else
            {
                return _PolicyFunction.Field(path, policyCxt);
            }
        }

        private object Left_Value(JsonElement element, Dictionary<string, object> context)
        {
            return this._PolicyFunction.Evaluate(element.GetString(), context);
        }
        private object Left_Count(JsonElement element, Dictionary<string, object> context)
        {
            var r = Count(element, context);
            if (r == -1)// https://docs.microsoft.com/en-us/azure/governance/policy/concepts/definition-structure#count
                return false;
            return r;
        }
        #endregion

        #region RightBuiltInCodition

        private bool EqualsMethod(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!EqualsMethod(item, right))
                        return false;
                }
                return true;
            }
            return string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private bool NotEquals(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!NotEquals(item, right))
                        return false;
                }
                return true;
            }
            return !string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private bool Like(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!Like(item, right))
                        return false;
                }
                return true;
            }
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
        }

        private bool NotLike(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!NotLike(item, right))
                        return false;
                }
                return true;
            }
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
        }

        private bool Match(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!Match(item, right))
                        return false;
                }
                return true;
            }
            return ConditionMatch(left.ToString(), right.ToString(), false);
        }

        private bool MatchInsensitively(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!MatchInsensitively(item, right))
                        return false;
                }
                return true;
            }
            return ConditionMatch(left.ToString(), right.ToString(), true);
        }

        private bool NotMatch(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!NotMatch(item, right))
                        return false;
                }
                return true;
            }
            return !ConditionMatch(left.ToString(), right.ToString(), false);
        }

        private bool NotMatchInsensitively(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!NotMatchInsensitively(item, right))
                        return false;
                }
                return true;
            }
            return !ConditionMatch(left.ToString(), right.ToString(), true);
        }

        private bool ContainsMethod(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!ContainsMethod(item, right))
                        return false;
                }
                return true;
            }
            return left.ToString().Contains(right.ToString());
        }

        private bool NotContains(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!NotContains(item, right))
                        return false;
                }
                return true;
            }
            return !left.ToString().Contains(right.ToString());
        }

        private bool In(object left, object right)
        {
            if (left == null || right == null) return false;
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!In(item, right))
                        return false;
                }
                return true;
            }
            if (right is List<object> rList)
            {
                return rList.Contains(left);
            }
            if (right is JsonValue jv)
            {
                return jv.Contains(left);
            }
            return false;
        }

        private bool NotIn(object left, object right)
        {
            if (left == null || right == null) return true;
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!NotIn(item, right))
                        return false;
                }
                return true;
            }
            if (right is List<object> rList)
            {
                return !rList.Contains(left);
            }
            if (right is JsonValue jv)
            {
                return !jv.Contains(left);
            }
            return true;
        }

        private bool ContainsKey(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!ContainsKey(item, right))
                        return false;
                }
                return true;
            }
            return (left as JsonValue).Contains(right);
        }

        private bool NotContainsKey(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!NotContainsKey(item, right))
                        return false;
                }
                return true;
            }
            return !(left as JsonValue).Contains(right);
        }

        private bool Less(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!Less(item, right))
                        return false;
                }
                return true;
            }
            return ConditionCompare(left.ToString(), right.ToString(), "less");
        }

        private bool LessOrEquals(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!LessOrEquals(item, right))
                        return false;
                }
                return true;
            }
            return ConditionCompare(left.ToString(), right.ToString(), "lessOrEquals");
        }

        private bool Greater(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!Greater(item, right))
                        return false;
                }
                return true;
            }
            return ConditionCompare(left.ToString(), right.ToString(), "greater");
        }

        private bool GreaterOrEquals(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!GreaterOrEquals(item, right))
                        return false;
                }
                return true;
            }
            return ConditionCompare(left.ToString(), right.ToString(), "greaterOrEquals");
        }

        private bool Exists(object left, object right)
        {
            if (left is List<object> list)
            {
                foreach (var item in list)
                {
                    if (!Exists(item, right))
                        return false;
                }
                return true;
            }
            var leftResult = (left != null) && ((int)left != -1);
            bool rightResult = (right.ToString().ToLower()) switch
            {
                "true" => true,
                "false" => false,
                _ => throw new Exception("input is not a valid bool string"),
            };
            return (leftResult == rightResult);
        }

        private void InitRightBuiltInCodition()
        {
            this._RightConditions.Add("equals", EqualsMethod);
            this._RightConditions.Add("notEquals", NotEquals);
            // When using the like and notLike conditions, you provide a wildcard * in the value. The value shouldn't have more than one wildcard *.
            this._RightConditions.Add("like", Like);
            this._RightConditions.Add("notLike", NotLike);
            // When using the match and notMatch conditions, provide # to match a digit, ? for a letter, . to match any character, and any other character to match that actual character. While match and notMatch are case-sensitive, all other conditions that evaluate a stringValue are case-insensitive. Case-insensitive alternatives are available in matchInsensitively and notMatchInsensitively.
            this._RightConditions.Add("match", Match);
            this._RightConditions.Add("matchInsensitively", MatchInsensitively);
            this._RightConditions.Add("notMatch", NotMatch);
            this._RightConditions.Add("notMatchInsensitively", NotMatchInsensitively);
            this._RightConditions.Add("contains", ContainsMethod);
            this._RightConditions.Add("notContains", NotContains);
            this._RightConditions.Add("in", In);
            this._RightConditions.Add("notIn", NotIn);
            this._RightConditions.Add("containsKey", ContainsKey);
            this._RightConditions.Add("notContainsKey", NotContainsKey);
            this._RightConditions.Add("less", Less);
            this._RightConditions.Add("lessOrEquals", LessOrEquals);
            this._RightConditions.Add("greater", Greater);
            this._RightConditions.Add("greaterOrEquals", GreaterOrEquals);
            this._RightConditions.Add("exists", Exists);
        }

        #endregion BuiltInCodition

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

            #endregion note

            object left = null;
            object right = null;
            Func<object, object, bool> func = null;
            var policyCxt = context[Functions.ContextKeys.POLICY_CONTEXT] as PolicyContext;
            var deployCxt = policyCxt.Resource.Deployment;

            foreach (var item in element.EnumerateObject())
            {
                if (this._LeftConditions.TryGetValue(item.Name, out Func<JsonElement, Dictionary<string, object>, object> left_func))
                {
                    left = left_func(item.Value, context);
                }
                else if (_RightConditions.TryGetValue(item.Name, out func))
                {
                    right = item.Value.GetEvaluatedValue(_PolicyFunction, context);
                }
            }
            if (func == null)
                throw new Exception("cannot find conditions");
            return func(left, right);
        }

        public int Count(JsonElement element, Dictionary<string, object> context)
        {
            if (!element.TryGetProperty("field", out JsonElement fieldE))
                return -1;
            var policyCxt = context[Functions.ContextKeys.POLICY_CONTEXT] as PolicyContext;
            var deployCxt = policyCxt.Resource.Deployment;

            var path = this._PolicyFunction.Evaluate(fieldE.GetString(), context).ToString();
            if (!(_PolicyFunction.Field(path, policyCxt) is List<object> d))
                return 0;
            if (element.TryGetProperty("where", out JsonElement whereE))
            {
                var logical = this._ServiceProvider.GetService<Logical>();
                return d.Where(e =>
                {
                    return logical.Evaluate(whereE, new Dictionary<string, object>() {
                        {Functions.ContextKeys.POLICY_CONTEXT,policyCxt },
                        {Functions.ContextKeys.COUNT_ELEMENT,e },
                        {Functions.ContextKeys.COUNT_FIELD,path }
                    });
                }).Count();
            }

            return d.Count();
        }

        private bool ConditionMatch(string left, string right, bool insensitively)
        {
            if (left.Length != right.Length)
                return false;
            int matched = 0;
            for (int i = 0; i < right.Length;)
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

        private bool ConditionCompare(string left, string right, string operation)
        {
            bool leftParse = DateTime.TryParseExact(left, "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime leftTime);
            bool rightParse = DateTime.TryParseExact(right, "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime rightTime);
            if (((leftParse == false) && (rightParse == true)) || ((leftParse == true) && (rightParse == false)))
            {
                throw new Exception($"field and input of condition can not both parse to datetime,field:{left},input:{right}");
            }
            //only both are datetime
            if (leftParse && rightParse)
            {
                return operation switch
                {
                    "less" => leftTime < rightTime,
                    "lessOrEquals" => leftTime <= rightTime,
                    "greater" => leftTime > rightTime,
                    "greaterOrEquals" => leftTime >= rightTime,
                    _ => throw new NotImplementedException(),
                };
            }

            leftParse = Int32.TryParse(left, out int leftInt);
            rightParse = Int32.TryParse(right, out int rightInt);
            if (((!leftParse) && rightParse) || (leftParse && (!rightParse)))
            {
                throw new Exception($"field and input of condition can not both parse to int,field:{left},input:{right}");
            }
            //only both are int
            if (leftParse && rightParse)
            {
                return operation switch
                {
                    "less" => leftInt < rightInt,
                    "lessOrEquals" => leftInt <= rightInt,
                    "greater" => leftInt > rightInt,
                    "greaterOrEquals" => leftInt >= rightInt,
                    _ => throw new NotImplementedException(),
                };
            }
            //end with string compare
            return operation switch
            {
                "less" => (string.Compare(left, right) < 0),
                "lessOrEquals" => (string.Compare(left, right) <= 0),
                "greater" => (string.Compare(left, right) > 0),
                "greaterOrEquals" => (string.Compare(left, right) >= 0),
                _ => throw new NotImplementedException(),
            };
        }
    }
}