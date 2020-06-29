using maskx.ARMOrchestration.Orchestrations;
using Xunit;

namespace AzurePolicyTest
{
    [Collection("WebHost PolicyService")]
    [Trait("c", "Condition")]
    public class ConditionTest
    {
        private readonly PolicyServiceFixture fixture;
        public ConditionTest(PolicyServiceFixture fixture)
        {
            this.fixture = fixture;
        }
        // TODO: Add test code
        [Fact(DisplayName = "in")]
        public void In()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "In",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vnet")
            });
            Assert.False(rtv.Result);
            Assert.Equal(2,rtv.DeniedPolicy.Count);
        }
        [Fact(DisplayName = "notIn")]
        public void NotIn()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "NotIn",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vnet")
            });
            Assert.True(rtv.Result);
            Assert.Empty(rtv.DeniedPolicy);
        }
    }
}
