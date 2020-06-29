using maskx.ARMOrchestration.Orchestrations;
using Xunit;

namespace AzurePolicyTest
{
    [Collection("WebHost PolicyService")]
    [Trait("c", "Effect")]
    public class EffectTest
    {
        private readonly PolicyServiceFixture fixture;
        public EffectTest(PolicyServiceFixture fixture)
        {
            this.fixture = fixture;
        }
        [Fact(DisplayName = "Disabled")]
        public void Disabled()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Effect",
                ResourceGroup = "Disabled",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "Deny")]
        public void Deny()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Effect",
                ResourceGroup = "Deny",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
    }
}
