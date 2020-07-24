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
            Assert.NotNull(rtv.DeploymentOrchestrationInput);
            Assert.NotNull(rtv.PolicyContext);

            // rtv.PolicyContext.Resource
            using var pc = JsonDocument.Parse(rtv.PolicyContext.Resource);
            Assert.True(pc.RootElement.TryGetProperty("properties", out JsonElement properties_pc));
            Assert.True(properties_pc.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup_pc));
            Assert.False(networkSecurityGroup_pc.TryGetProperty("id", out JsonElement _));

            //rtv.DeploymentOrchestrationInput.TemplateContent
            using var doc = JsonDocument.Parse(rtv.DeploymentOrchestrationInput.TemplateContent);
            Assert.True(doc.RootElement.TryGetProperty("resources", out JsonElement resources));
            foreach (var r in resources.EnumerateArray())
            {
                Assert.True(r.TryGetProperty("properties", out JsonElement properties));
                Assert.True(properties.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup));
                Assert.False(networkSecurityGroup.TryGetProperty("id", out JsonElement id));
            }

            // rtv.DeploymentOrchestrationInput.Template
            using var p = JsonDocument.Parse(rtv.DeploymentOrchestrationInput.Template.Resources["Subnet1"].Properties);
            Assert.True(p.RootElement.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup1));
            Assert.False(networkSecurityGroup1.TryGetProperty("id", out JsonElement _));
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
            Assert.NotNull(rtv.DeploymentOrchestrationInput);

            // rtv.PolicyContext.Resource
            using var doc = JsonDocument.Parse(rtv.PolicyContext.Resource);
            Assert.True(doc.RootElement.TryGetProperty("properties", out JsonElement properties));
            Assert.True(properties.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup));
            Assert.True(networkSecurityGroup.TryGetProperty("id", out JsonElement _));

            // rtv.DeploymentOrchestrationInput.TemplateContent
            using var doc_dp = JsonDocument.Parse(rtv.DeploymentOrchestrationInput.TemplateContent);
            Assert.True(doc_dp.RootElement.TryGetProperty("resources", out JsonElement resources));
            foreach (var r in resources.EnumerateArray())
            {
                Assert.True(r.TryGetProperty("properties", out JsonElement properties_dp));
                Assert.True(properties_dp.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup_dp));
                Assert.True(networkSecurityGroup_dp.TryGetProperty("id", out JsonElement _));
            }

            // rtv.DeploymentOrchestrationInput.Template.Resources
            using var p = JsonDocument.Parse(rtv.DeploymentOrchestrationInput.Template.Resources["Subnet1"].Properties);
            Assert.True(p.RootElement.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup_p));
            Assert.True(networkSecurityGroup_p.TryGetProperty("id", out JsonElement _));
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
            Assert.NotNull(rtv.DeploymentOrchestrationInput);
            Assert.NotNull(rtv.PolicyContext);
            // rtv.PolicyContext.Resource
            using var doc_pc = JsonDocument.Parse(rtv.PolicyContext.Resource);
            Assert.True(doc_pc.RootElement.TryGetProperty("properties", out JsonElement properties_pc));
            Assert.False(properties_pc.TryGetProperty("serviceEndpoints", out JsonElement _));

            // rtv.DeploymentOrchestrationInput.TemplateContent
            using var doc = JsonDocument.Parse(rtv.DeploymentOrchestrationInput.TemplateContent);
            Assert.True(doc.RootElement.TryGetProperty("resources", out JsonElement resources));
            foreach (var r in resources.EnumerateArray())
            {
                Assert.True(r.TryGetProperty("properties", out JsonElement properties));
                Assert.False(properties.TryGetProperty("serviceEndpoints", out JsonElement _));
            }

            // rtv.DeploymentOrchestrationInput.Template.Resources
            using var p = JsonDocument.Parse(rtv.DeploymentOrchestrationInput.Template.Resources["Subnet1"].Properties);
            Assert.False(p.RootElement.TryGetProperty("serviceEndpoints", out JsonElement _));

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
            Assert.NotNull(rtv.DeploymentOrchestrationInput);

            // rtv.PolicyContext.Resource
            using var doc = JsonDocument.Parse(rtv.PolicyContext.Resource);
            Assert.True(doc.RootElement.TryGetProperty("properties", out JsonElement properties));
            Assert.True(properties.TryGetProperty("serviceEndpoints", out JsonElement _));


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
            Assert.NotNull(rtv.DeploymentOrchestrationInput);

            // rtv.PolicyContext.Resource
            using var doc = JsonDocument.Parse(rtv.PolicyContext.Resource);
            Assert.True(doc.RootElement.TryGetProperty("properties", out JsonElement properties));
            Assert.True(properties.TryGetProperty("addressPrefixes", out JsonElement addressPrefixes));
            foreach (var item in addressPrefixes.EnumerateArray())
            {
                Assert.NotEqual("10.0.0.0/16", item.GetString());
            }

            // rtv.DeploymentOrchestrationInput.TemplateContent
            using var doc_dp = JsonDocument.Parse(rtv.DeploymentOrchestrationInput.TemplateContent);
            Assert.True(doc_dp.RootElement.TryGetProperty("resources", out JsonElement resources));
            foreach (var r in resources.EnumerateArray())
            {
                Assert.True(r.TryGetProperty("properties", out JsonElement properties_dp));
                Assert.True(properties_dp.TryGetProperty("addressPrefixes", out JsonElement addressPrefixes_dp));
                foreach (var item in addressPrefixes_dp.EnumerateArray())
                {
                    Assert.NotEqual("10.0.0.0/16", item.GetString());
                }
            }

            // rtv.DeploymentOrchestrationInput.Template.Resources
            using var p = JsonDocument.Parse(rtv.DeploymentOrchestrationInput.Template.Resources["Subnet1"].Properties);
            Assert.True(p.RootElement.TryGetProperty("addressPrefixes", out JsonElement addressPrefixes_p));
            foreach (var item in addressPrefixes_p.EnumerateArray())
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
            Assert.NotNull(rtv.PolicyContext);

            // rtv.PolicyContext.Resource
            using var doc = JsonDocument.Parse(rtv.PolicyContext.Resource);
            Assert.True(doc.RootElement.TryGetProperty("properties", out JsonElement properties));
            Assert.True(properties.TryGetProperty("subnets", out JsonElement subnets));
            foreach (var subnet in subnets.EnumerateArray())
            {
                if (subnet.TryGetProperty("properties", out JsonElement subnetproperties))
                {
                    Assert.False(subnetproperties.TryGetProperty("addressPrefix", out JsonElement _));
                }
            }

            // rtv.DeploymentOrchestrationInput.TemplateContent
            using var doc_dp = JsonDocument.Parse(rtv.DeploymentOrchestrationInput.TemplateContent);
            Assert.True(doc_dp.RootElement.TryGetProperty("resources", out JsonElement resources));
            foreach (var r in resources.EnumerateArray())
            {
                Assert.True(r.TryGetProperty("properties", out JsonElement properties_dp));
                Assert.True(properties_dp.TryGetProperty("subnets", out JsonElement subnets_dp));
                foreach (var subnet in subnets_dp.EnumerateArray())
                {
                    if (subnet.TryGetProperty("properties", out JsonElement subnetproperties_dp))
                    {
                        Assert.False(subnetproperties_dp.TryGetProperty("addressPrefix", out JsonElement _));
                    }
                }
            }

            // rtv.DeploymentOrchestrationInput.Template.Resources
            using var p = JsonDocument.Parse(rtv.DeploymentOrchestrationInput.Template.Resources["VNet1"].Properties);
            Assert.True(p.RootElement.TryGetProperty("subnets", out JsonElement subnets_p));
            foreach (var subnet in subnets_p.EnumerateArray())
            {
                if (subnet.TryGetProperty("properties", out JsonElement subnetproperties_p))
                {
                    Assert.False(subnetproperties_p.TryGetProperty("addressPrefix", out JsonElement _));
                }
            }
        }
    }
}
