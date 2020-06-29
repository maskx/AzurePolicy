using maskx.AzurePolicy.Definitions;
using System.Collections.Generic;

namespace maskx.AzurePolicy.Services
{
    public class ValidationResult
    {
        public bool Result { get; set; }
        public string Message { get; set; }
        public string Template { get; set; }
        public List<PolicyDefinition> DeniedPolicy { get; set; } = new List<PolicyDefinition>();
    }
}
