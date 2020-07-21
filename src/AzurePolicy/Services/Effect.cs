using maskx.ARMOrchestration.Functions;
using maskx.ARMOrchestration.Orchestrations;
using maskx.AzurePolicy.Definitions;
using maskx.AzurePolicy.Extensions;
using maskx.AzurePolicy.Functions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace maskx.AzurePolicy.Services
{
    public class Effect
    {
        public const string DisabledEffectName = "disabled";
        public const string DenyEffectName = "deny";
        public const string ModifyEffectName = "modify";
        public const string DeployIfNotExistsEffectName = "deployifnotexists";
        public const int DefaultPriority = 600;

        private readonly Dictionary<string, Action<string, Dictionary<string, object>>> _Effects = new Dictionary<string, Action<string, Dictionary<string, object>>>();
        private readonly Dictionary<string, int> _EffectPriority = new Dictionary<string, int>();
        private readonly PolicyFunction _PolicyFunction;
        private readonly ARMFunctions _ARMFunctions;
        private readonly ARMOrchestration.IInfrastructure _ARMInfrastructure;

        public Effect(PolicyFunction policyFunction,
            ARMFunctions aRMFunctions,
            ARMOrchestration.IInfrastructure aRMInfrastructure )
        {
            this._PolicyFunction = policyFunction;
            this._ARMFunctions = aRMFunctions;
            this._ARMInfrastructure = aRMInfrastructure;
            InitBuiltInEffects();
        }
        private void InitBuiltInEffects()
        {
            this._EffectPriority.Add(DisabledEffectName, 0);
            this._EffectPriority.Add(ModifyEffectName, 100);
            this._EffectPriority.Add(DenyEffectName, 200);
            this._EffectPriority.Add(DeployIfNotExistsEffectName, 300);

            //  use modify effect insteade append effect
            // https://docs.microsoft.com/en-us/azure/governance/policy/concepts/effects#modify
            this._Effects.Add("modify", (detail, context) =>
            {
                var input = context[Functions.ContextKeys.DEPLOY_CONTEXT] as DeploymentOrchestrationInput;
                var policyCxt = context[Functions.ContextKeys.POLICY_CONTEXT] as PolicyContext;
                var resource = JObject.Parse(policyCxt.Resource);
                var properties = resource["properties"] as JObject;
                using var doc = JsonDocument.Parse(detail);
                var operations = doc.RootElement.GetProperty("operations");
                foreach (var item in operations.EnumerateArray())
                {
                    switch (item.GetProperty("operation").GetString())
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
                policyCxt.Resource = resource.ToString();

                // modify resource information in DeploymentOrchestrationInput 
                string key = "";
                input.Template.Resources[key] = ARMOrchestration.ARMTemplate.Resource.Parse(
                    policyCxt.Resource,
                    new Dictionary<string, object>() { },
                    _ARMFunctions,
                    _ARMInfrastructure,
                    "",
                    ""
                    )[0];
            });
            // TODO: DeployIfNotExists Effect 
            // https://docs.microsoft.com/en-us/azure/governance/policy/concepts/effects#deployifnotexists
            this._Effects.Add("DeployIfNotExists", (detail, context) =>
            {

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
            var field = _PolicyFunction.Evaluate(operation.GetProperty("field").ToString(), context).ToString();
            properties.RemoveToken(GetPathArray(field));// 当字符串中包含 . 时 回出错

        }
        private string[] GetPathArray(string path)
        {
            List<string> paths = new List<string>();
            string p = string.Empty;
            bool singleQuote = false;
            bool doubleQuote = false;
            var span = path.AsSpan();
            for (int i = 0; i < span.Length; i++)
            {

                if (span[i] == '\\')
                {
                    i++;
                }
                else if (span[i] == '\'')
                {
                    singleQuote = !singleQuote;
                }
                else if (span[i] == '\"')
                {
                    doubleQuote = !doubleQuote;
                }
                else if (span[i] == '.')
                {
                    if (!(singleQuote || doubleQuote))
                    {
                        paths.Add(p);
                        p = string.Empty;
                        continue;
                    }
                }
                p += span[i];
            }
            paths.Add(p);
            return paths.ToArray();
        }
        public void SetEffect(string name, Action<string, Dictionary<string, object>> func)
        {
            this._Effects[name] = func;
        }
        public void Run(PolicyContext policyContext, DeploymentContext deploymentContext)
        {
            if (!this._Effects.TryGetValue(policyContext.PolicyDefinition.EffectName, out Action<string, Dictionary<string, object>> func))
                throw new Exception($"cannot find an effect named '{policyContext.PolicyDefinition.EffectName}'");
            func(policyContext.PolicyDefinition.EffectDetail,
                new Dictionary<string, object>() {
                    { Functions.ContextKeys.POLICY_CONTEXT,policyContext},
                    {Functions.ContextKeys.DEPLOY_CONTEXT,deploymentContext }
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
