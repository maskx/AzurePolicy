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
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Logical_not",
                Template = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }

        [Fact(DisplayName = "allOf")]
        public void AllOf()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Logical_allOf",
                Template = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }

        [Fact(DisplayName = "anyOf")]
        public void AnyOf()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Logical_anyOf",
                Template = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }
    }
}