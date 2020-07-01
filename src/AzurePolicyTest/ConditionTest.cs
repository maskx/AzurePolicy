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
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "equals",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.Single( rtv.DeniedPolicy);
        }
     
        [Fact(DisplayName = "notEquals")]
        public void NotEquals()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "NotEquals",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.Empty(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "like")]
        public void Like()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "Like",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "notLike")]
        public void NotLike()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "notLike",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.Empty(rtv.DeniedPolicy);

        }
        [Fact(DisplayName = "in")]
        public void In()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "In",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vnet")
            });
            Assert.False(rtv.Result);
            Assert.Equal(2,rtv.DeniedPolicy.Count);
        }
        [Fact(DisplayName = "notIn")]
        public void NotIn()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "NotIn",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vnet")
            });
            Assert.True(rtv.Result);
            Assert.Empty(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "match")]
        public void ConditionMatch()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "match",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "notMatch")]
        public void NotMatch()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "NotMatch",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.Empty(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "matchInsensitively")]
        public void ConditionMatchInsensitively()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "matchInsensitively",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "notMatchInsensitively")]
        public void NotMatchInsensitively()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "notMatchInsensitively",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.Empty(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "contains")]
        public void Contains()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "Contains",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/count")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "notContains")]
        public void NotContains()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "NotContains",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/count")
            });
            Assert.True(rtv.Result);
            Assert.Empty(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "less")]
        public void Less()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "less",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "greater")]
        public void Greater()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "Greater",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.Empty(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "lessOrEquals")]
        public void LessOrEquals()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "lessOrEquals",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "greaterOrEquals")]
        public void GreaterOrEquals()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "GreaterOrEquals",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.Empty(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "containsKey")]
        public void ContainsKey()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "containsKey",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "notContainsKey")]
        public void NotContainsKey()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "notContainsKey",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.Empty(rtv.DeniedPolicy);
        }
        [Fact(DisplayName = "exists")]
        public void Exists()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentContext()
            {
                SubscriptionId = "Condition",
                ResourceGroup = "exists",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.Single(rtv.DeniedPolicy);
        }
    }
}
