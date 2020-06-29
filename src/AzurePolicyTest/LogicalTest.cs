using maskx.ARMOrchestration.Orchestrations;
using Xunit;

namespace AzurePolicyTest
{
    [Collection("WebHost PolicyService")]
    [Trait("c", "Logical")]
    public class LogicalTest
    {
        private readonly PolicyServiceFixture fixture;
        public LogicalTest(PolicyServiceFixture fixture)
        {
            this.fixture = fixture;
        }
        [Fact(DisplayName = "not")]
        public void Not()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Logical",
                ResourceGroup = "not",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "allOf")]
        public void AllOf()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Logical",
                ResourceGroup = "allOf",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "anyOf")]
        public void AnyOf()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Logical",
                ResourceGroup = "anyOf",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
    }
}
