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
        
    }
}
