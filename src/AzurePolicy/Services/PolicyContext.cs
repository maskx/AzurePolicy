using maskx.AzurePolicy.Definitions;
using System.Text.Json;

namespace maskx.AzurePolicy.Services
{
    public class PolicyContext
    {
        public PolicyDefinition PolicyDefinition { get; set; }
        public string Parameters { get; set; }
        public string Resource { get; set; }
        public string NamePath { get; set; }
    }
}
