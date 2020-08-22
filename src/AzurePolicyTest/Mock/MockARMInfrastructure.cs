using maskx.ARMOrchestration;
using maskx.ARMOrchestration.Activities;
using maskx.ARMOrchestration.Orchestrations;
using maskx.OrchestrationService;
using maskx.OrchestrationService.Activity;
using System;
using System.Collections.Generic;

namespace AzurePolicyTest.Mock
{
    public class MockARMInfrastructure : IInfrastructure
    {
        public BuiltinServiceTypes BuiltinServiceTypes { get; set; } = new BuiltinServiceTypes()
        {
            Deployments = "Microsoft.Resources/deployments"
        };

        public BuiltinPathSegment BuiltinPathSegment { get; set; } = new BuiltinPathSegment();
        public bool InjectBeforeDeployment { get; set; } = false;
        public bool InjectAfterDeployment { get; set; } = false;
        public bool InjectBefroeProvisioning { get; set; } = false;
        public bool InjectAfterProvisioning { get; set; } = false;
        public List<(string Name, string Version)> BeforeDeploymentOrchestration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<(string Name, string Version)> AfterDeploymentOrhcestration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<(string Name, string Version)> BeforeResourceProvisioningOrchestation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<(string Name, string Version)> AfterResourceProvisioningOrchestation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public (string GroupId, string GroupType, string HierarchyId) GetGroupInfo(string managementGroupId, string subscriptionId, string resourceGroupName)
        {
            return ("D51C2231-3D30-4FEB-BEBB-EDBE081106DA", "ResourceGroup", "001002003004");
        }

        public AsyncRequestInput GetRequestInput(AsyncRequestActivityInput input)
        {
            throw new NotImplementedException();
        }

        public TaskResult List(DeploymentOrchestrationInput context, string resourceId, string apiVersion, string functionValues = "", string value = "")
        {
            throw new NotImplementedException();
        }

        public TaskResult Providers(string providerNamespace, string resourceType)
        {
            throw new NotImplementedException();
        }

        public TaskResult Reference(DeploymentOrchestrationInput context, string resourceName, string apiVersion = "", bool full = false)
        {
            throw new NotImplementedException();
        }

        public TaskResult WhatIf(DeploymentOrchestrationInput context, string resourceName)
        {
            throw new NotImplementedException();
        }
    }
}