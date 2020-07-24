using maskx.AzurePolicy.Definitions;

namespace maskx.AzurePolicy.Services
{
    public class PolicyContext
    {
        public PolicyDefinition PolicyDefinition { get; set; }
        public string Parameters { get; set; }
        public string Resource { get; set; }
        public string NamePath { get; set; } = string.Empty;
        public string ParentType { get; set; } = string.Empty;
        public EvaluatingPhase EvaluatingPhase { get; set; }
    }
}
