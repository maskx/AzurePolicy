using maskx.AzurePolicy.Functions;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace maskx.AzurePolicy.Services
{
    public class Logical
    {
        private readonly Dictionary<string, Func<JsonElement, Dictionary<string, object>, bool>> Logicals = new Dictionary<string, Func<JsonElement, Dictionary<string, object>, bool>>();
        private readonly Condition _Condition;

        public Logical(Condition condition)
        {
            this._Condition = condition;
            InitBuiltInLogicals();
        }

        private void InitBuiltInLogicals()
        {
            this.Logicals.Add("not", (arg, cxt) =>
            {
                return !this.Evaluate(arg.GetProperty("not"), cxt);
            });
            this.Logicals.Add("allOf", (arg, cxt) =>
            {
                foreach (var item in arg.GetProperty("allOf").EnumerateArray())
                {
                    if (!this.Evaluate(item, cxt))
                        return false;
                }
                return true;
            });
            this.Logicals.Add("anyOf", (arg, cxt) =>
            {
                foreach (var item in arg.GetProperty("anyOf").EnumerateArray())
                {
                    if (this.Evaluate(item, cxt))
                        return true;
                }
                return false;
            });
        }
        // todo: 需要重新设计，支持 返回命中条件信息，以便拼接策略执行结果
        public bool Evaluate(JsonElement element, Dictionary<string, object> context)
        {
            foreach (var item in element.EnumerateObject())
            {
                if (this.Logicals.TryGetValue(item.Name, out Func<JsonElement, Dictionary<string, object>, bool> func))
                {
                    return func(element, context);
                }
                else
                {
                    return this._Condition.Evaluate(element, context);
                }
            }
            throw new Exception();
        }

        public bool Evaluate(PolicyContext policyContext)
        {
            using var doc = JsonDocument.Parse(policyContext.PolicyDefinition.PolicyRule.If);
            Dictionary<string, object> context = new Dictionary<string, object>()
            {
                { ContextKeys.POLICY_CONTEXT,policyContext }
            };
            return Evaluate(doc.RootElement, context);
        }
    }
}