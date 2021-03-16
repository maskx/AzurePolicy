using maskx.ARMOrchestration;
using Xunit;

namespace AzurePolicyTest
{
    [Collection("WebHost PolicyService")]
    [Trait("c", "Field")]
    public class FieldTest
    {
        private readonly PolicyServiceFixture fixture;

        public FieldTest(PolicyServiceFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact(DisplayName = "name")]
        public void Name()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Field_name",
                Template = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }

        [Fact(DisplayName = "type")]
        public void Type()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Field_type",
                Template = TestHelper.GetJsonFileContent("json/template/vm")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }

        [Fact(DisplayName = "fullName")]
        public void FullName()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Field_fullName",
                Template = TestHelper.GetJsonFileContent("json/template/vnet")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }
    }
}