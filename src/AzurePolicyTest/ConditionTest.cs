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
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_equals",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.NotNull( rtv.PolicyContext);
        }
     
        [Fact(DisplayName = "notEquals")]
        public void NotEquals()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_NotEquals",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.Null(rtv.PolicyContext);
        }
        [Fact(DisplayName = "like")]
        public void Like()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_Like",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }
        [Fact(DisplayName = "notLike")]
        public void NotLike()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_notLike",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.Null(rtv.PolicyContext);

        }
        [Fact(DisplayName = "in")]
        public void In()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_In",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vnet")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }
        [Fact(DisplayName = "notIn")]
        public void NotIn()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_NotIn",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vnet")
            });
            Assert.True(rtv.Result);
            Assert.Null(rtv.PolicyContext);
        }
        [Fact(DisplayName = "match")]
        public void ConditionMatch()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_match",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }
        [Fact(DisplayName = "notMatch")]
        public void NotMatch()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_NotMatch",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.Null(rtv.PolicyContext);
        }
        [Fact(DisplayName = "matchInsensitively")]
        public void ConditionMatchInsensitively()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_matchInsensitively",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }
        [Fact(DisplayName = "notMatchInsensitively")]
        public void NotMatchInsensitively()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_notMatchInsensitively",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.Null(rtv.PolicyContext);
        }
        [Fact(DisplayName = "contains")]
        public void Contains()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_Contains",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/count")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }
        [Fact(DisplayName = "notContains")]
        public void NotContains()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_NotContains",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/count")
            });
            Assert.True(rtv.Result);
            Assert.Null(rtv.PolicyContext);
        }
        [Fact(DisplayName = "less")]
        public void Less()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_less",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }
        [Fact(DisplayName = "greater")]
        public void Greater()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_Greater",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.Null(rtv.PolicyContext);
        }
        [Fact(DisplayName = "lessOrEquals")]
        public void LessOrEquals()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_lessOrEquals",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }
        [Fact(DisplayName = "greaterOrEquals")]
        public void GreaterOrEquals()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_GreaterOrEquals",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.Null(rtv.PolicyContext);
        }
        [Fact(DisplayName = "containsKey")]
        public void ContainsKey()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_containsKey",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }
        [Fact(DisplayName = "notContainsKey")]
        public void NotContainsKey()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_notContainsKey",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.Null(rtv.PolicyContext);
        }
        [Fact(DisplayName = "exists")]
        public void Exists()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "Condition_exists",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.False(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
        }
    }
}
