using maskx.ARMOrchestration.Orchestrations;
using Xunit;

namespace AzurePolicyTest
{
    [Collection("WebHost PolicyService")]
    [Trait("c", "Function")]
    public class PolicyFunctionTest
    {
        private readonly PolicyServiceFixture fixture;
        public PolicyFunctionTest(PolicyServiceFixture fixture)
        {
            this.fixture = fixture;
        }
        [Fact(DisplayName = "field")]
        public void Field()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Function",
                ResourceGroup = "Field",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "fieldInLength")]
        public void FieldInLength()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Function",
                ResourceGroup = "FieldInLength",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/count")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "addDays")]
        public void AddDays()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Function",
                ResourceGroup = "addDays",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/count")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
    }
}
