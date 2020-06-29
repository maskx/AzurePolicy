namespace maskx.AzurePolicy.Definitions
{
    /// <summary>
    /// <see cref="https://docs.microsoft.com/en-us/azure/governance/policy/concepts/definition-structure#type"/>
    /// <seealso cref=""/>
    /// </summary>
    public class PolicyRule
    {
        public string If { get; set; }
        public string Then { get; set; }       
    }
}
