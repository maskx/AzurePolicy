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
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Function_Field",
                Template = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }

        [Fact(DisplayName = "fieldInLength")]
        public void FieldInLength()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Function_FieldInLength",
                Template = TestHelper.GetJsonFileContent("json/template/count")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }

        [Fact(DisplayName = "addDays")]
        public void AddDays()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Function_addDays",
                Template = TestHelper.GetJsonFileContent("json/template/count")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }
    }
}