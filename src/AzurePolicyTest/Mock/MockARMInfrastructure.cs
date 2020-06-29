using maskx.ARMOrchestration;
using maskx.ARMOrchestration.Activities;
using maskx.ARMOrchestration.Orchestrations;
using maskx.OrchestrationService;
using maskx.OrchestrationService.Activity;
using System;
using System.Collections.Generic;

namespace AzurePolicyTest.Mock
{
    public class MockARMInfrastructure : maskx.ARMOrchestration.IInfrastructure
    {
        public BuiltinServiceTypes BuitinServiceTypes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public BuiltinPathSegment BuiltinPathSegment { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool InjectBeforeDeployment { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool InjectAfterDeployment { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool InjectBefroeProvisioning { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool InjectAfterProvisioning { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<(string Name, string Version)> BeforeDeploymentOrchestration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<(string Name, string Version)> AfterDeploymentOrhcestration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<(string Name, string Version)> BeforeResourceProvisioningOrchestation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<(string Name, string Version)> AfterResourceProvisioningOrchestation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public AsyncRequestInput GetRequestInput(AsyncRequestActivityInput input)
        {
            throw new NotImplementedException();
        }

        public TaskResult List(DeploymentContext context, string resourceId, string apiVersion, string functionValues = "", string value = "")
        {
            throw new NotImplementedException();
        }

        public TaskResult Providers(string providerNamespace, string resourceType)
        {
            throw new NotImplementedException();
        }

        public TaskResult Reference(DeploymentContext context, string resourceName, string apiVersion = "", bool full = false)
        {
            throw new NotImplementedException();
        }

        public TaskResult WhatIf(DeploymentContext context, string resourceName)
        {
            throw new NotImplementedException();
        }
    }
}
