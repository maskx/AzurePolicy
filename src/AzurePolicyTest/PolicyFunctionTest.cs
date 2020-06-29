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
        [Fact(DisplayName = "Test1")]
        public void Test1()
        {
            this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = TestHelper.ResourceGroup,
                TemplateContent= TestHelper.GetJsonFileContent("json/template/vm")
            });
        }
    }
}
