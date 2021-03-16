using maskx.ARMOrchestration;
using maskx.ARMOrchestration.Orchestrations;
using Xunit;

namespace AzurePolicyTest
{
    [Collection("WebHost PolicyService")]
    [Trait("c", "Condition")]
    public class ConditionTest
    {
        private readonly PolicyServiceFixture fixture;

        public ConditionTest(PolicyServiceFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact(DisplayName = "equals")]
        public void ConditionEquals()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_equals",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
        }

        [Fact(DisplayName = "notEquals")]
        public void NotEquals()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_NotEquals",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
        }

        [Fact(DisplayName = "like")]
        public void Like()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_Like",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
        }

        [Fact(DisplayName = "notLike")]
        public void NotLike()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_notLike",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
        }

        [Fact(DisplayName = "in")]
        public void In()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_In",
                Template = TestHelper.GetJsonFileContent("json/template/vnet")
            });
            Assert.False(rtv.Result);
        }

        [Fact(DisplayName = "inWithField")]
        public void InWithField()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_InWithField",
                Template = TestHelper.GetJsonFileContent("json/template/vnet")
            });
            Assert.False(rtv.Result);
        }

        [Fact(DisplayName = "notIn")]
        public void NotIn()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_NotIn",
                Template = TestHelper.GetJsonFileContent("json/template/vnet")
            });
            Assert.True(rtv.Result);
        }

        [Fact(DisplayName = "match")]
        public void ConditionMatch()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_match",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
        }

        [Fact(DisplayName = "notMatch")]
        public void NotMatch()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_NotMatch",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
        }

        [Fact(DisplayName = "matchInsensitively")]
        public void ConditionMatchInsensitively()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_matchInsensitively",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
        }

        [Fact(DisplayName = "notMatchInsensitively")]
        public void NotMatchInsensitively()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_notMatchInsensitively",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
        }

        [Fact(DisplayName = "contains")]
        public void Contains()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_Contains",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
        }

        [Fact(DisplayName = "notContains")]
        public void NotContains()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_NotContains",
                Template = TestHelper.GetJsonFileContent("json/template/count")
            });
            Assert.True(rtv.Result);
        }

        [Fact(DisplayName = "less")]
        public void Less()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_less",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
        }

        [Fact(DisplayName = "greater")]
        public void Greater()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_Greater",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
        }

        [Fact(DisplayName = "lessOrEquals")]
        public void LessOrEquals()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_lessOrEquals",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
        }

        [Fact(DisplayName = "greaterOrEquals")]
        public void GreaterOrEquals()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_GreaterOrEquals",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
        }

        [Fact(DisplayName = "containsKey")]
        public void ContainsKey()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_containsKey",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
        }

        [Fact(DisplayName = "notContainsKey")]
        public void NotContainsKey()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_notContainsKey",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
        }

        [Fact(DisplayName = "exists")]
        public void Exists()
        {
            var rtv = this.fixture.PolicyService.Validate(new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_exists",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
        }
    }
}