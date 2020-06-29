using maskx.ARMOrchestration.Orchestrations;
using Xunit;

namespace AzurePolicyTest
{
    [Collection("WebHost PolicyService")]
    [Trait("c", "Count")]
    public class CountTest
    {
        private readonly PolicyServiceFixture fixture;
        public CountTest(PolicyServiceFixture fixture)
        {
            this.fixture = fixture;
        }
        [Fact(DisplayName = "CountNoWhere")]
        public void CountNoWhere()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Count",
                ResourceGroup = "NoWhere",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/count")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "CountWhere")]
        public void CountWhere()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Count",
                ResourceGroup = "Where",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/count")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "CountWhereWithChildProperty")]
        public void CountWhereWithChildProperty()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Count",
                ResourceGroup = "WhereWithChildProperty",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/count_child_property")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
    }
}
