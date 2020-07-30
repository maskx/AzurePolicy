﻿using maskx.ARMOrchestration.ARMTemplate;
using maskx.ARMOrchestration.Functions;
using maskx.ARMOrchestration.Orchestrations;
using maskx.AzurePolicy.Definitions;
using maskx.AzurePolicy.Extensions;
using maskx.AzurePolicy.Functions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

namespace maskx.AzurePolicy.Services
{
    public class Effect
    {
        public const string DisabledEffectName = "disabled";
        public const string DenyEffectName = "deny";
        public const string ModifyEffectName = "modify";
        public const string DeployIfNotExistsEffectName = "deployifnotexists";
        public const string DenyIfNotExistsName = "denyifnotexists";
        public const int DefaultPriority = 600;

        private readonly Dictionary<string, Func<string, Dictionary<string, object>, bool>> _Effects = new Dictionary<string, Func<string, Dictionary<string, object>, bool>>();
        private readonly Dictionary<string, int> _EffectPriority = new Dictionary<string, int>();
        private readonly PolicyFunction _PolicyFunction;
        private readonly ARMFunctions _ARMFunctions;
        private readonly ARMOrchestration.IInfrastructure _ARMInfrastructure;
        private readonly IInfrastructure _PolicyInfrastructure;
        private readonly Logical _Logical;

        public Effect(PolicyFunction policyFunction,
            ARMFunctions aRMFunctions,
            ARMOrchestration.IInfrastructure aRMInfrastructure,
            IInfrastructure infrastructure,
            Logical logical)
        {
            this._PolicyInfrastructure = infrastructure;
            this._Logical = logical;
            this._PolicyFunction = policyFunction;
            this._ARMFunctions = aRMFunctions;
            this._ARMInfrastructure = aRMInfrastructure;
            InitBuiltInEffects();
        }

        private void InitBuiltInEffects()
        {
            this._EffectPriority.Add(DisabledEffectName, 0);
            this._EffectPriority.Add(ModifyEffectName, 100);
            this._EffectPriority.Add(DeployIfNotExistsEffectName, 200);
            this._EffectPriority.Add(DenyEffectName, 900);
            this._EffectPriority.Add(DenyIfNotExistsName, 900);

            this._Effects.Add(DisabledEffectName, (detail, context) => false);

            //  use modify effect insteade append effect
            // https://docs.microsoft.com/en-us/azure/governance/policy/concepts/effects#modify
            this._Effects.Add(ModifyEffectName, Modify);
            this._Effects.Add(DeployIfNotExistsEffectName, (detail, context) => { return true; });
            this._Effects.Add(DenyEffectName, (detai, context) => false);
            this._Effects.Add(DenyIfNotExistsName, ResourceIsExists);
        }

        #region ResourceIfNotExists

        public bool ResourceIsExists(string detail, Dictionary<string, object> context)
        {
            string name = null, existenceScope = null;
            JsonElement? existenceCondition = null;
            using var doc = JsonDocument.Parse(detail);
            var root = doc.RootElement;
            string type;
            if (!root.TryGetProperty("type", out JsonElement TypeE))
                throw new Exception("cannot find type property");
            else
                type = _PolicyFunction.Evaluate(TypeE.GetString(), context).ToString();
            if (root.TryGetProperty("name", out JsonElement nameE))
                name = _PolicyFunction.Evaluate(nameE.GetString(), context).ToString();
            string resourceGroupName;
            if (root.TryGetProperty("resourceGroupName", out JsonElement rgE))
                resourceGroupName = _PolicyFunction.Evaluate(rgE.GetString(), context).ToString();
            else
                resourceGroupName = (context[Functions.ContextKeys.DEPLOY_CONTEXT] as DeploymentOrchestrationInput).ResourceGroup;
            if (root.TryGetProperty("existenceScope", out JsonElement existenceScopeE))
                existenceScope = _PolicyFunction.Evaluate(existenceScopeE.GetString(), context).ToString();
            if (root.TryGetProperty("existenceCondition", out JsonElement ExistenceConditionE))
                existenceCondition = ExistenceConditionE;
            if (FindInTemplate(type, name, resourceGroupName, existenceCondition, context))
                return true;
            if (_PolicyInfrastructure.ResourceIsExisting(type, name, resourceGroupName, existenceScope, existenceCondition.HasValue ? existenceCondition.Value.ExpandObject(context, _PolicyFunction) : null))
                return true;
            return false;
        }

        private bool FindInResourceCollection(ResourceCollection resources, string type, string name, JsonElement? condition, Dictionary<string, object> context)
        {
            var policyContext = context[Functions.ContextKeys.POLICY_CONTEXT] as PolicyContext;
            var deployContext = context[Functions.ContextKeys.DEPLOY_CONTEXT] as DeploymentOrchestrationInput;
            if (resources.Any((r) =>
            {
                if (r.Type != type)
                    return false;
                if (!string.IsNullOrEmpty(name) && r.Name != name)
                    return false;
                if (!condition.HasValue)
                    return true;
                return this._Logical.Evaluate(condition.Value, new Dictionary<string, object>() {
                        {Functions.ContextKeys.DEPLOY_CONTEXT,deployContext },
                        {Functions.ContextKeys.POLICY_CONTEXT,new PolicyContext(){
                            EvaluatingPhase=policyContext.EvaluatingPhase,
                            NamePath=policyContext.NamePath,
                            Parameters=policyContext.Parameters,
                            ParentType=policyContext.ParentType,
                            PolicyDefinition=policyContext.PolicyDefinition,
                            Resource=r.ToString(),
                            RootInput=policyContext.RootInput
                        } } });
            }))
                return true;
            return false;
        }

        private bool FindInTemplate(string type, string name, string rg, JsonElement? condition, Dictionary<string, object> context)
        {
            var policyContext = context[Functions.ContextKeys.POLICY_CONTEXT] as PolicyContext;
            var input = policyContext.RootInput;
            if (input.ResourceGroup == rg)
            {
                if (FindInResourceCollection(input.Template.Resources, type, name, condition, context))
                    return true;
            }
            foreach (var d in input.Deployments.Values)
            {
                if (d.ResourceGroup == rg)
                {
                    if (FindInResourceCollection(d.Template.Resources, type, name, condition, context))
                        return true;
                }
            }
            return false;
        }

        #endregion

        #region Modify

        private bool Modify(string detail, Dictionary<string, object> context)
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
            var template = JObject.Parse(input.TemplateContent);
            // modify resource information in DeploymentOrchestrationInput 
            var r = GetResourceByPath(template["resources"] as JArray,
                policyCxt.NamePath.Split('/').Append(resource["name"].ToString()).ToArray());
            r.Replace(resource);
            input.TemplateContent = template.ToString(Newtonsoft.Json.Formatting.Indented);
            var res = Resource.Parse(policyCxt.Resource,
                new Dictionary<string, object> {
                        { ARMOrchestration.Functions.ContextKeys.ARM_CONTEXT, input }
                },
                _ARMFunctions,
                _ARMInfrastructure,
                policyCxt.NamePath,
                policyCxt.ParentType);
            foreach (var rr in res)
            {
                input.Template.Resources.Remove(rr);
                input.Template.Resources.Add(rr);
            }
            return true;
        }

        private JObject GetResourceByPath(JArray jarray, string[] path)
        {
            JToken rtv = null;
            JArray children = jarray;
            foreach (var p in path)
            {
                foreach (var child in children)
                {
                    if (child["name"].ToString() == p)
                    {
                        rtv = child;
                        children = rtv["resources"] as JArray;
                        break;
                    }
                }
            }
            return rtv as JObject;
        }

        private void ModifyAddOperation(JObject properties, JsonElement operation, Dictionary<string, object> context)
        {
            var field = _PolicyFunction.Evaluate(operation.GetProperty("field").ToString(), context).ToString();
            var value = JToken.Parse(operation.GetProperty("value").GetRawText());
            properties.Add(GetPathArray(field), value);
        }

        private void ModifyAddOrReplaceOperation(JObject properties, JsonElement operation, Dictionary<string, object> context)
        {
            var field = _PolicyFunction.Evaluate(operation.GetProperty("field").ToString(), context).ToString();
            JToken value = JToken.Parse(operation.GetProperty("value").GetRawText());
            properties.AddOrRepleace(GetPathArray(field), value);

        }

        private void ModifyRemoveOperation(JObject properties, JsonElement operation, Dictionary<string, object> context)
        {
            var field = _PolicyFunction.Evaluate(operation.GetProperty("field").ToString(), context).ToString();
            properties.RemoveToken(GetPathArray(field));

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

        #endregion

        public void SetEffect(string name, Func<string, Dictionary<string, object>, bool> func)
        {
            this._Effects[name] = func;
        }

        public bool Run(PolicyContext policyContext, DeploymentContext deploymentContext)
        {
            if (!this._Effects.TryGetValue(policyContext.PolicyDefinition.EffectName.ToLower(), out Func<string, Dictionary<string, object>, bool> func))
                throw new Exception($"cannot find an effect named '{policyContext.PolicyDefinition.EffectName}'");
            return func(policyContext.PolicyDefinition.EffectDetail,
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
