using maskx.ARMOrchestration;
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
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Effect_Disabled",
                Template = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
        }

        [Fact(DisplayName = "Deny")]
        public void Deny()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Effect_Deny",
                Template = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
        }

        [Fact(DisplayName = "DenyIfNotExists_ExistsInTemplate")]
        public void DenyIfNotExists_ExistsInTemplate()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "DenyIfNotExists_ExistsInTemplate",
                Template = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.True(rtv.Result);
        }

        [Fact(DisplayName = "DenyIfNotExists_ExistsInInfrastructure")]
        public void DenyIfNotExists_ExistsInInfrastructure()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "DenyIfNotExists_ExistsInInfrastructure",
                Template = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.True(rtv.Result);
        }

        [Fact(DisplayName = "DenyIfNotExists_ExistsInTemplateWithCondition")]
        public void DenyIfNotExists_ExistsInTemplateWithCondition()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "DenyIfNotExists_ExistsInTemplateWithCondition",
                Template = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.True(rtv.Result);
        }

        [Fact(DisplayName = "DenyIfNotExists_ExistsInInfrastructureWithCondition")]
        public void DenyIfNotExists_ExistsInInfrastructureWithCondition()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "DenyIfNotExists_ExistsInInfrastructureWithCondition",
                Template = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.True(rtv.Result);
        }
    }
}