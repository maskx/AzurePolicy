using maskx.ARMOrchestration.Orchestrations;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AzurePolicyTest
{
    [Collection("WebHost PolicyService")]
    public class PolicyFunctionTest
    {
        private readonly PolicyServiceFixture fixture;
        public PolicyFunctionTest(PolicyServiceFixture fixture)
        {
            this.fixture = fixture;
        }
        [Fact(DisplayName = "Test1")]
        public void Test1()
        {
            string template = TestHelper.GetJsonFileContent("json/arm/vm");
            this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = TestHelper.ResourceGroup,
                TemplateContent=""
            });
        }
    }
}
