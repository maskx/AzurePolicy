using maskx.ARMOrchestration.ARMTemplate;
using maskx.AzurePolicy.Definitions;

namespace maskx.AzurePolicy.Services
{
    public class PolicyContext
    {
        public PolicyDefinition PolicyDefinition { get; set; }
        public string Parameters { get; set; }
        public Resource Resource { get; set; }
        public EvaluatingPhase EvaluatingPhase { get; set; }
    }
}