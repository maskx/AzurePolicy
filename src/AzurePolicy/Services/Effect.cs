﻿using maskx.ARMOrchestration.Orchestrations;
using maskx.AzurePolicy.Definitions;
using maskx.AzurePolicy.Functions;
using Newtonsoft.Json.Linq;
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
            //  use modify effect insteade append effect
            this._Effects.Add("modify", (detail, context) =>
            {
                var deployCxt = context[ContextKeys.DEPLOY_CONTEXT] as DeploymentContext;
                var policyCxt = context[ContextKeys.POLICY_CONTEXT] as PolicyContext;
                var resource = JObject.Parse(policyCxt.Resource);
                var properties = resource["properties"] as JObject;
                using var doc = JsonDocument.Parse(detail);
                var operations = doc.RootElement.GetProperty("operations");
                foreach (var item in operations.EnumerateArray())
                {
                    switch (item.GetProperty("").GetString().ToLower())
                    {
                        case "addOrReplace":
                            ModifyAddOrReplaceOperation(properties, item, context);
                            break;
                        case "Add":
                            ModifyAddOperation(properties, item, context);
                            break;
                        case "Remove":
                            ModifyRemoveOperation(properties, item, context);
                            break;
                        default:
                            break;
                    }

                }
                var template = JObject.Parse(deployCxt.TemplateContent);
                string resourceName = resource["Name"].ToString();
                foreach (var item in (template["resources"] as JArray))
                {
                    if (item["name"].ToString() == resourceName)
                    {
                        break;
                    }
                }
                return "";
            });
        }
        // TODO: ModifyAddOperation
        private void ModifyAddOperation(JObject properties, JsonElement operation, Dictionary<string, object> context)
        {

            var field = _PolicyFunction.Evaluate(operation.GetProperty("field").ToString(), context).ToString();
            var value = operation.GetProperty("value").GetRawText();
            field = field.Remove(0, field.LastIndexOf('/') + 1);
            if (field.EndsWith("[*]"))
            {
                field = field.Remove(0, field.Length - 3);
            }
            else
            {
                // TODO: 需要考虑 节点 不存在，需要新增的情况
                var p = properties.SelectToken(field) as JArray;
                p.Add(JToken.Parse(value));
            }
        }
        // ModifyAddOrReplaceOperation
        private void ModifyAddOrReplaceOperation(JObject properties, JsonElement operation, Dictionary<string, object> context)
        {

        }
        // ModifyRemoveOperation
        private void ModifyRemoveOperation(JObject properties, JsonElement operation, Dictionary<string, object> context)
        {

        }
        public void SetEffect(string name, Func<string, Dictionary<string, object>, string> func)
        {
            this._Effects[name] = func;
        }
        public string Run(PolicyContext policyContext, DeploymentContext deploymentContext)
        {
            if (!this._Effects.TryGetValue(policyContext.PolicyDefinition.EffectName, out Func<string, Dictionary<string, object>, string> func))
                throw new Exception($"cannot find an effect named '{policyContext.PolicyDefinition.EffectName}'");
            return func(policyContext.PolicyDefinition.EffectDetail,
                new Dictionary<string, object>() {
                    { ContextKeys.POLICY_CONTEXT,policyContext},
                    {ContextKeys.DEPLOY_CONTEXT,deploymentContext }
                });
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
