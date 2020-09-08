using System.Collections.Generic;

namespace maskx.AzurePolicy.Definitions
{
    public class PolicyAssignment
    {
        public string Scope { get; set; }
        public List<string> NotScopes { get; set; }
    }
}
