using maskx.ARMOrchestration.Orchestrations;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AzurePolicyTest
{
    [Collection("WebHost PolicyService")]
    [Trait("c", "Field")]
    public class FieldTest
    {
        private readonly PolicyServiceFixture fixture;
        public FieldTest(PolicyServiceFixture fixture)
        {
            this.fixture = fixture;
        }
        [Fact(DisplayName ="name")]
        public void Name()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "field",
                ResourceGroup = "name",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "type")]
        public void Type()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "field",
                ResourceGroup = "type",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "fullName")]
        public void FullName()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "field",
                ResourceGroup = "fullName",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vnet")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
    }
}
