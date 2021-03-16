using maskx.ARMOrchestration;
using System.Linq;
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

        #region remove

        [Fact(DisplayName = "RmoveJobject")]
        public void RmoveJobject()
        {
            var deployment = new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_RmoveJobject",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            };
            var rtv = this.fixture.PolicyService.Validate(deployment);
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);

            // rtv.PolicyContext.Resource
            using var pc = JsonDocument.Parse(rtv.PolicyContext.Resource.ToString());
            Assert.True(pc.RootElement.TryGetProperty("properties", out JsonElement properties_pc));
            Assert.True(properties_pc.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup_pc));
            Assert.False(networkSecurityGroup_pc.TryGetProperty("id", out JsonElement _));

            //rtv.DeploymentOrchestrationInput.Template
            using var doc = JsonDocument.Parse(deployment.Template.ToString());
            Assert.True(doc.RootElement.TryGetProperty("resources", out JsonElement resources));
            foreach (var r in resources.EnumerateArray())
            {
                Assert.True(r.TryGetProperty("properties", out JsonElement properties));
                Assert.True(properties.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup));
                Assert.False(networkSecurityGroup.TryGetProperty("id", out JsonElement id));
            }

            // rtv.DeploymentOrchestrationInput.Template
            using var p = JsonDocument.Parse(deployment.Template.Resources["Subnet1"].Properties);
            Assert.True(p.RootElement.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup1));
            Assert.False(networkSecurityGroup1.TryGetProperty("id", out JsonElement _));
        }

        [Fact(DisplayName = "RmoveJobjectNotExist")]
        public void RmoveJobjectNotExist()
        {
            var deployment = new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_RmoveJobjectNotExist",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            };
            var rtv = this.fixture.PolicyService.Validate(deployment);
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);

            // rtv.PolicyContext.Resource
            using var doc = JsonDocument.Parse(rtv.PolicyContext.Resource.ToString());
            Assert.True(doc.RootElement.TryGetProperty("properties", out JsonElement properties));
            Assert.True(properties.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup));
            Assert.True(networkSecurityGroup.TryGetProperty("id", out JsonElement _));

            // rtv.DeploymentOrchestrationInput.TemplateContent
            using var doc_dp = JsonDocument.Parse(deployment.Template.ToString());
            Assert.True(doc_dp.RootElement.TryGetProperty("resources", out JsonElement resources));
            foreach (var r in resources.EnumerateArray())
            {
                Assert.True(r.TryGetProperty("properties", out JsonElement properties_dp));
                Assert.True(properties_dp.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup_dp));
                Assert.True(networkSecurityGroup_dp.TryGetProperty("id", out JsonElement _));
            }

            // rtv.DeploymentOrchestrationInput.Template.Resources
            using var p = JsonDocument.Parse(deployment.Template.Resources["Subnet1"].Properties);
            Assert.True(p.RootElement.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup_p));
            Assert.True(networkSecurityGroup_p.TryGetProperty("id", out JsonElement _));
        }

        [Fact(DisplayName = "RmoveArray")]
        public void RmoveArray()
        {
            var deployment = new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_RmoveArray",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            };
            var rtv = this.fixture.PolicyService.Validate(deployment);
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
            // rtv.PolicyContext.Resource
            using var doc_pc = JsonDocument.Parse(rtv.PolicyContext.Resource.ToString());
            Assert.True(doc_pc.RootElement.TryGetProperty("properties", out JsonElement properties_pc));
            Assert.False(properties_pc.TryGetProperty("serviceEndpoints", out JsonElement _));

            // rtv.DeploymentOrchestrationInput.TemplateContent
            using var doc = JsonDocument.Parse(deployment.Template.ToString());
            Assert.True(doc.RootElement.TryGetProperty("resources", out JsonElement resources));
            foreach (var r in resources.EnumerateArray())
            {
                Assert.True(r.TryGetProperty("properties", out JsonElement properties));
                Assert.False(properties.TryGetProperty("serviceEndpoints", out JsonElement _));
            }

            // rtv.DeploymentOrchestrationInput.Template.Resources
            using var p = JsonDocument.Parse(deployment.Template.Resources["Subnet1"].Properties);
            Assert.False(p.RootElement.TryGetProperty("serviceEndpoints", out JsonElement _));
        }

        [Fact(DisplayName = "RmoveArrayNotExist")]
        public void RmoveArrayNotExist()
        {
            var deployment = new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_RmoveArrayNotExist",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            };
            var rtv = this.fixture.PolicyService.Validate(deployment);
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);

            // rtv.PolicyContext.Resource
            using var doc = JsonDocument.Parse(rtv.PolicyContext.Resource.ToString());
            Assert.True(doc.RootElement.TryGetProperty("properties", out JsonElement properties));
            Assert.True(properties.TryGetProperty("serviceEndpoints", out JsonElement _));
        }

        [Fact(DisplayName = "RmoveStringItemInArray")]
        public void RmoveStringItemInArray()
        {
            var deployment = new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_RmoveStringItemInArray",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            };
            var rtv = this.fixture.PolicyService.Validate(deployment);
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);

            // rtv.PolicyContext.Resource
            using var doc = JsonDocument.Parse(rtv.PolicyContext.Resource.ToString());
            Assert.True(doc.RootElement.TryGetProperty("properties", out JsonElement properties));
            Assert.True(properties.TryGetProperty("addressPrefixes", out JsonElement addressPrefixes));
            foreach (var item in addressPrefixes.EnumerateArray())
            {
                Assert.NotEqual("10.0.0.0/16", item.GetString());
            }

            // rtv.DeploymentOrchestrationInput.TemplateContent
            using var doc_dp = JsonDocument.Parse(deployment.Template.ToString());
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
            using var p = JsonDocument.Parse(deployment.Template.Resources["Subnet1"].Properties);
            Assert.True(p.RootElement.TryGetProperty("addressPrefixes", out JsonElement addressPrefixes_p));
            foreach (var item in addressPrefixes_p.EnumerateArray())
            {
                Assert.NotEqual("10.0.0.0/16", item.GetString());
            }
        }

        [Fact(DisplayName = "RmoveOjbjectInArray")]
        public void RmoveOjbjectInArray()
        {
            var deployment = new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_RmoveOjbjectInArray",
                Template = TestHelper.GetJsonFileContent("json/template/vnet")
            };
            var rtv = this.fixture.PolicyService.Validate(deployment);
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);

            // rtv.DeploymentOrchestrationInput.Template.Resources
            using var p1 = JsonDocument.Parse(deployment.Template.Resources["VNet1"].ToString());
            Assert.True(p1.RootElement.TryGetProperty("properties", out JsonElement p2));
            Assert.True(p2.TryGetProperty("subnets", out JsonElement subnets_p1));
            foreach (var subnet in subnets_p1.EnumerateArray())
            {
                if (subnet.TryGetProperty("properties", out JsonElement subnetproperties_p1))
                {
                    Assert.False(subnetproperties_p1.TryGetProperty("addressPrefix", out JsonElement _));
                }
            }

            // rtv.DeploymentOrchestrationInput.TemplateContent
            using var doc_dp = JsonDocument.Parse(deployment.Template.ToString());
            Assert.True(doc_dp.RootElement.TryGetProperty("resources", out JsonElement resources));
            foreach (var r in resources.EnumerateArray())
            {
                if (r.GetProperty("type").GetString() != "Microsoft.Network/virtualNetworks")
                    continue;
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

            // rtv.DeploymentOrchestrationInput.Template.Resources.Properties
            using var p = JsonDocument.Parse(deployment.Template.Resources["VNet1"].Properties);
            Assert.True(p.RootElement.TryGetProperty("subnets", out JsonElement subnets_p));
            foreach (var subnet in subnets_p.EnumerateArray())
            {
                if (subnet.TryGetProperty("properties", out JsonElement subnetproperties_p))
                {
                    Assert.False(subnetproperties_p.TryGetProperty("addressPrefix", out JsonElement _));
                }
            }
        }

        #endregion remove

        #region AddOrReplaceOperation

        [Fact(DisplayName = "AddOrReplace_AddJobject")]
        public void AddOrReplace_AddJobject()
        {
            var deployment = new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_AddOrReplace_AddJobject",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            };
            var rtv = this.fixture.PolicyService.Validate(deployment);
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);

            // rtv.PolicyContext.Resource
            using var pc = JsonDocument.Parse(rtv.PolicyContext.Resource.ToString());
            Assert.True(pc.RootElement.TryGetProperty("properties", out JsonElement properties_pc));
            Assert.True(properties_pc.TryGetProperty("routeTable", out JsonElement routeTable_pc));
            Assert.True(routeTable_pc.TryGetProperty("id", out JsonElement v_pc));
            Assert.Equal("abc", v_pc.GetString());

            //rtv.DeploymentOrchestrationInput.TemplateContent
            using var doc = JsonDocument.Parse(deployment.Template.ToString());
            Assert.True(doc.RootElement.TryGetProperty("resources", out JsonElement resources));
            foreach (var r in resources.EnumerateArray())
            {
                Assert.True(r.TryGetProperty("properties", out JsonElement properties));
                Assert.True(properties.TryGetProperty("routeTable", out JsonElement routeTable));
                Assert.True(routeTable.TryGetProperty("id", out JsonElement id));
                Assert.Equal("abc", id.GetString());
            }

            // rtv.DeploymentOrchestrationInput.Template
            using var p = JsonDocument.Parse(deployment.Template.Resources["Subnet1"].Properties);
            Assert.True(p.RootElement.TryGetProperty("routeTable", out JsonElement routeTable_t));
            Assert.True(routeTable_t.TryGetProperty("id", out JsonElement id_t));
            Assert.Equal("abc", id_t.GetString());
        }

        [Fact(DisplayName = "AddOrReplace_ReplaceJobject")]
        public void AddOrReplace_ReplaceJobject()
        {
            var deployment = new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_AddOrReplace_ReplaceJobject",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            };
            var rtv = this.fixture.PolicyService.Validate(deployment);
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);

            // rtv.PolicyContext.Resource
            using var pc = JsonDocument.Parse(rtv.PolicyContext.Resource.ToString());
            Assert.True(pc.RootElement.TryGetProperty("properties", out JsonElement properties_pc));
            Assert.True(properties_pc.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup_pc));
            Assert.True(networkSecurityGroup_pc.TryGetProperty("id", out JsonElement v_pc));
            Assert.Equal("abc", v_pc.GetString());

            //rtv.DeploymentOrchestrationInput.TemplateContent
            using var doc = JsonDocument.Parse(deployment.Template.ToString());
            Assert.True(doc.RootElement.TryGetProperty("resources", out JsonElement resources));
            foreach (var r in resources.EnumerateArray())
            {
                Assert.True(r.TryGetProperty("properties", out JsonElement properties));
                Assert.True(properties.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup));
                Assert.True(networkSecurityGroup.TryGetProperty("id", out JsonElement id));
                Assert.Equal("abc", id.GetString());
            }

            // rtv.DeploymentOrchestrationInput.Template
            using var p = JsonDocument.Parse(deployment.Template.Resources["Subnet1"].Properties);
            Assert.True(p.RootElement.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup_t));
            Assert.True(networkSecurityGroup_t.TryGetProperty("id", out JsonElement id_t));
            Assert.Equal("abc", id_t.GetString());
        }

        [Fact(DisplayName = "AddOrReplace_AddItemInArray")]
        public void AddOrReplace_AddItemInArray()
        {
            var deployment = new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_AddOrReplace_AddItemInArray",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            };
            var rtv = this.fixture.PolicyService.Validate(deployment);
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
            // rtv.PolicyContext.Resource
            using var doc_pc = JsonDocument.Parse(rtv.PolicyContext.Resource.ToString());
            Assert.True(doc_pc.RootElement.TryGetProperty("properties", out JsonElement properties_pc));
            Assert.True(properties_pc.TryGetProperty("addressPrefixes", out JsonElement addressPrefixes_pc));
            Assert.Equal(1, addressPrefixes_pc.EnumerateArray().Count((e) => e.GetString() == "10.0.0.1/12"));

            // rtv.DeploymentOrchestrationInput.TemplateContent
            using var doc = JsonDocument.Parse(deployment.Template.ToString());
            Assert.True(doc.RootElement.TryGetProperty("resources", out JsonElement resources));
            foreach (var r in resources.EnumerateArray())
            {
                Assert.True(r.TryGetProperty("properties", out JsonElement properties));
                Assert.True(properties.TryGetProperty("addressPrefixes", out JsonElement addressPrefixes));
                Assert.Equal(1, addressPrefixes.EnumerateArray().Count((e) => e.GetString() == "10.0.0.1/12"));
            }

            // rtv.DeploymentOrchestrationInput.Template.Resources
            using var p = JsonDocument.Parse(deployment.Template.Resources["Subnet1"].Properties);
            Assert.True(p.RootElement.TryGetProperty("addressPrefixes", out JsonElement addressPrefixes_t));
            Assert.Equal(1, addressPrefixes_t.EnumerateArray().Count((e) => e.GetString() == "10.0.0.1/12"));
        }

        //[Fact(DisplayName = "AddOrReplace_ReplaceItemInArray")]
        //public void AddOrReplace_ReplaceItemInArray()
        //{
        //    var rtv = this.fixture.PolicyService.Validate(new DeploymentOrchestrationInput()
        //    {
        //        SubscriptionId = TestHelper.SubscriptionId,
        //        ResourceGroup = "ModifyEffect_AddOrReplace_ReplaceItemInArray",
        //        TemplateContent = TestHelper.GetJsonFileContent("json/template/subnet")
        //    });
        //    Assert.True(rtv.Result);
        //    Assert.NotNull(rtv.DeploymentOrchestrationInput);
        //    Assert.NotNull(rtv.PolicyContext);
        //    // rtv.PolicyContext.Resource
        //    using var doc_pc = JsonDocument.Parse(rtv.PolicyContext.Resource);
        //    Assert.True(doc_pc.RootElement.TryGetProperty("properties", out JsonElement properties_pc));
        //    Assert.True(properties_pc.TryGetProperty("addressPrefixes", out JsonElement addressPrefixes_pc));
        //    Assert.Equal(2, addressPrefixes_pc.EnumerateArray().Count());

        //    // rtv.DeploymentOrchestrationInput.TemplateContent
        //    using var doc = JsonDocument.Parse(rtv.DeploymentOrchestrationInput.TemplateContent);
        //    Assert.True(doc.RootElement.TryGetProperty("resources", out JsonElement resources));
        //    foreach (var r in resources.EnumerateArray())
        //    {
        //        Assert.True(r.TryGetProperty("properties", out JsonElement properties));
        //        Assert.True(properties.TryGetProperty("addressPrefixes", out JsonElement addressPrefixes));
        //        Assert.Equal(2, addressPrefixes.EnumerateArray().Count());
        //    }

        //    // rtv.DeploymentOrchestrationInput.Template.Resources
        //    using var p = JsonDocument.Parse(rtv.DeploymentOrchestrationInput.Template.Resources["Subnet1"].Properties);
        //    Assert.True(p.RootElement.TryGetProperty("addressPrefixes", out JsonElement addressPrefixes_t));
        //    Assert.Equal(2, addressPrefixes_t.EnumerateArray().Count());
        //}

        [Fact(DisplayName = "AddOrReplace_AddItemInArrayOfArray")]
        public void AddOrReplace_AddItemInArrayOfArray()
        {
            var deployment = new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_AddOrReplace_AddItemInArrayOfArray",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            };
            var rtv = this.fixture.PolicyService.Validate(deployment);
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);
            // rtv.PolicyContext.Resource
            using var doc_pc = JsonDocument.Parse(rtv.PolicyContext.Resource.ToString());
            Assert.True(doc_pc.RootElement.TryGetProperty("properties", out JsonElement properties_pc));
            Assert.True(properties_pc.TryGetProperty("serviceEndpoints", out JsonElement serviceEndpoints_pc));
            foreach (var item in serviceEndpoints_pc.EnumerateArray())
            {
                Assert.True(item.TryGetProperty("locations", out JsonElement locations_pc));
                Assert.Equal(1, locations_pc.EnumerateArray().Count((e) => e.GetString() == "xyz"));
            }

            // rtv.DeploymentOrchestrationInput.TemplateContent
            using var doc = JsonDocument.Parse(deployment.Template.ToString());
            Assert.True(doc.RootElement.TryGetProperty("resources", out JsonElement resources));
            foreach (var r in resources.EnumerateArray())
            {
                Assert.True(r.TryGetProperty("properties", out JsonElement properties));
                Assert.True(properties.TryGetProperty("serviceEndpoints", out JsonElement serviceEndpoints));
                foreach (var item in serviceEndpoints.EnumerateArray())
                {
                    Assert.True(item.TryGetProperty("locations", out JsonElement locations));
                    Assert.Equal(1, locations.EnumerateArray().Count((e) => e.GetString() == "xyz"));
                }
            }

            // rtv.DeploymentOrchestrationInput.Template.Resources
            using var p = JsonDocument.Parse(deployment.Template.Resources["Subnet1"].Properties);
            Assert.True(p.RootElement.TryGetProperty("serviceEndpoints", out JsonElement serviceEndpoints_t));
            foreach (var item in serviceEndpoints_t.EnumerateArray())
            {
                Assert.True(item.TryGetProperty("locations", out JsonElement locations_t));
                Assert.Equal(1, locations_t.EnumerateArray().Count((e) => e.GetString() == "xyz"));
            }
        }

        #endregion AddOrReplaceOperation

        #region Add

        [Fact(DisplayName = "Add_JobjectExist")]
        public void Add_JobjectExist()
        {
            var deployment = new Deployment()
            {
                SubscriptionId = TestHelper.SubscriptionId,
                ResourceGroup = "ModifyEffect_Add_JobjectExist",
                Template = TestHelper.GetJsonFileContent("json/template/subnet")
            };
            var rtv = this.fixture.PolicyService.Validate(deployment);
            Assert.True(rtv.Result);
            Assert.NotNull(rtv.PolicyContext);

            // rtv.PolicyContext.Resource
            using var pc = JsonDocument.Parse(rtv.PolicyContext.Resource.ToString());
            Assert.True(pc.RootElement.TryGetProperty("properties", out JsonElement properties_pc));
            Assert.True(properties_pc.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup_pc));
            Assert.True(networkSecurityGroup_pc.TryGetProperty("id", out JsonElement v_pc));
            Assert.Equal("123", v_pc.GetString());

            //rtv.DeploymentOrchestrationInput.TemplateContent
            using var doc = JsonDocument.Parse(deployment.Template.ToString());
            Assert.True(doc.RootElement.TryGetProperty("resources", out JsonElement resources));
            foreach (var r in resources.EnumerateArray())
            {
                Assert.True(r.TryGetProperty("properties", out JsonElement properties));
                Assert.True(properties.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup));
                Assert.True(networkSecurityGroup.TryGetProperty("id", out JsonElement id));
                Assert.Equal("123", id.GetString());
            }

            // rtv.DeploymentOrchestrationInput.Template
            using var p = JsonDocument.Parse(deployment.Template.Resources["Subnet1"].Properties);
            Assert.True(p.RootElement.TryGetProperty("networkSecurityGroup", out JsonElement networkSecurityGroup_t));
            Assert.True(networkSecurityGroup_t.TryGetProperty("id", out JsonElement id_t));
            Assert.Equal("123", id_t.GetString());
        }

        #endregion Add
    }
}