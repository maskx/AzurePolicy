using maskx.AzurePolicy.Definitions;
using maskx.AzurePolicy.Functions;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace maskx.AzurePolicy.Services
{
    public class Effect
    {
        public const string DisabledEffectName = "disabled";
        public const string DenyEffectName = "deny";
        public const int DefaultPriority = 100;
        private readonly Dictionary<string, Func<string, Dictionary<string, object>, string>> _Effects = new Dictionary<string, Func<string, Dictionary<string, object>, string>>();
        private readonly Dictionary<string, int> _EffectPriority = new Dictionary<string, int>();
        private readonly PolicyFunction _PolicyFunction;
        public Effect(PolicyFunction policyFunction)
        {
            this._PolicyFunction = policyFunction;
            InitBuiltInEffects();
        }
        private void InitBuiltInEffects()
        {
            this._EffectPriority.Add(DisabledEffectName, 0);
            this._EffectPriority.Add(DenyEffectName, 200);
            this._Effects.Add("append", (detail, context) =>
            {
                return "";
            });
        }
        public void SetEffect(string name, Func<string, Dictionary<string, object>, string> func)
        {
            this._Effects[name] = func;
        }
        public string Run(string name, string detail, Dictionary<string, object> context)
        {
            if (!this._Effects.TryGetValue(name, out Func<string, Dictionary<string, object>, string> func))
                throw new Exception($"cannot find an effect named '{name}'");
            return func(detail, context);
        }
        public int ParseEffect(PolicyDefinition policyDefinition, Dictionary<string, object> context)
        {
            using var doc = JsonDocument.Parse(policyDefinition.PolicyRule.Then);
            var root = doc.RootElement;
            policyDefinition.EffectName = this._PolicyFunction.Evaluate(root.GetProperty("effect").GetString(), context).ToString();
            if (root.TryGetProperty("details", out JsonElement detailE))
                policyDefinition.EffectDetail = detailE.GetRawText();
            if (this._EffectPriority.TryGetValue(policyDefinition.EffectName, out int priority))
                policyDefinition.EffectPriority = priority;
            else
                policyDefinition.EffectPriority = DefaultPriority;
            return policyDefinition.EffectPriority;
        }
    }
}
