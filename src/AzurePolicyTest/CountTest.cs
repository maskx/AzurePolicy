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
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Count_NoWhere",
                Template = TestHelper.GetJsonFileContent("json/template/count")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }

        [Fact(DisplayName = "CountWhere")]
        public void CountWhere()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Count_Where",
                Template = TestHelper.GetJsonFileContent("json/template/count")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }

        [Fact(DisplayName = "CountWhereWithChildProperty")]
        public void CountWhereWithChildProperty()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Count_WhereWithChildProperty",
                Template = TestHelper.GetJsonFileContent("json/template/count_child_property")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }
    }
}