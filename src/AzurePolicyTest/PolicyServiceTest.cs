using maskx.ARMOrchestration;
using Xunit;

namespace AzurePolicyTest
{
    [Collection("WebHost PolicyService")]
    [Trait("c", "PolicyService")]
    public class PolicyServiceTest
    {
        private readonly PolicyServiceFixture fixture;

        public PolicyServiceTest(PolicyServiceFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact(DisplayName = "NestTemplate")]
        public void NestTemplate()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "PolicyService_nestTemplate",
                Template = TestHelper.GetJsonFileContent("json/template/nestTemplate")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }

        [Fact(DisplayName = "CopyResource")]
        public void CopyResource()
        {
            // todo: test CopyResource
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "PolicyService_CopyResource",
                Template = TestHelper.GetJsonFileContent("json/template/vnetWithCopy"),
                Name= "PolicyService_CopyResource"
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }
    }
}