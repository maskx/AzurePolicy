using maskx.ARMOrchestration.Orchestrations;

namespace maskx.AzurePolicy.Services
{
    public class ValidationResult
    {
        public bool Result { get; set; }
        public string Message { get; set; }
        public DeploymentOrchestrationInput DeploymentOrchestrationInput { get; set; }
        public PolicyContext PolicyContext { get; set; }
    }
}
