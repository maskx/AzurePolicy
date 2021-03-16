using maskx.ARMOrchestration;
using maskx.ARMOrchestration.Orchestrations;

namespace maskx.AzurePolicy.Services
{
    public class ValidationResult
    {
        /// <summary>
        /// pass policy evaluate or not
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// the evaluate message
        /// </summary>
        public string Message { get; set; }


        /// <summary>
        /// the last evaluated policyContext
        /// </summary>
        public PolicyContext PolicyContext { get; set; }
    }
}