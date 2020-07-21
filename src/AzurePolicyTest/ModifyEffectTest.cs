using maskx.ARMOrchestration.Orchestrations;
using System.Text.Json;
using Xunit;

namespace AzurePolicyTest
{
    [Collection("WebHost PolicyService")]
    [Trait("c", "Effect")]
    [Trait("Effect", "Modify")]
    public class ModifyEffectTest
    {
        private readonly PolicyServiceFixture fixture;
        public ModifyEffectTest(PolicyServiceFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact(DisplayName = "RmoveJobject")]
        public void RmoveJobject()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_RmoveJobject",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
            using var doc = JsonDocument.Parse(rtv.PolicyContext.Resource);
            Assert.True(doc.RootElement.TryGetProperty("properties", out JsonElement properties));
            Assert.True(properties.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup));
            Assert.False(networkSecurityGroup.TryGetProperty("id", out JsonElement id));
        }
        [Fact(DisplayName = "RmoveJobjectNotExist")]
        public void RmoveJobjectNotExist()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_RmoveJobjectNotExist",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
            using var doc = JsonDocument.Parse(rtv.PolicyContext.Resource);
            Assert.True(doc.RootElement.TryGetProperty("properties", out JsonElement properties));
            Assert.True(properties.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup));
            Assert.True(networkSecurityGroup.TryGetProperty("id", out JsonElement id));
        }
        [Fact(DisplayName = "RmoveArray")]
        public void RmoveArray()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_RmoveArray",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
            using var doc = JsonDocument.Parse(rtv.PolicyContext.Resource);
            Assert.True(doc.RootElement.TryGetProperty("properties", out JsonElement properties));
            Assert.False(properties.TryGetProperty("serviceEndpoints", out JsonElement serviceEndpoints));
        }
        [Fact(DisplayName = "RmoveArrayNotExist")]
        public void RmoveArrayNotExist()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_RmoveArrayNotExist",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
            using var doc = JsonDocument.Parse(rtv.PolicyContext.Resource);
            Assert.True(doc.RootElement.TryGetProperty("properties", out JsonElement properties));
            Assert.True(properties.TryGetProperty("serviceEndpoints", out JsonElement serviceEndpoints));
        }
        [Fact(DisplayName = "RmoveStringItemInArray")]
        public void RmoveStringItemInArray()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_RmoveStringItemInArray",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
            });
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
            using var doc = JsonDocument.Parse(rtv.PolicyContext.Resource);
            Assert.True(doc.RootElement.TryGetProperty("properties", out JsonElement properties));
            Assert.True(properties.TryGetProperty("addressPrefixes", out JsonElement addressPrefixes));
            foreach (var item in addressPrefixes.EnumerateArray())
            {
                Assert.NotEqual("10.0.0.0/16", item.GetString());
            }
        }
        [Fact(DisplayName = "RmoveOjbjectInArray")]
        public void RmoveOjbjectInArray()
        {
            var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_RmoveOjbjectInArray",
                TemplateContent = TestHelper.GetJsonFileContent("json/template/vnet")
            });
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.DeploymentOrchestrationInput);
            using var doc = JsonDocument.Parse(rtv.PolicyContext.Resource);
            Assert.True(doc.RootElement.TryGetProperty("properties", out JsonElement properties));
            Assert.True(properties.TryGetProperty("subnets", out JsonElement subnets));
            foreach (var subnet in subnets.EnumerateArray())
            {
                if (subnet.TryGetProperty("properties", out JsonElement subnetproperties))
                {
                    Assert.False(subnetproperties.TryGetProperty("addressPrefix", out JsonElement addressPrefix));
                }
            }
        }
    }
}
