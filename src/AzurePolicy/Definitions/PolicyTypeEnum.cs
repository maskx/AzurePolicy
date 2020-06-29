namespace maskx.AzurePolicy.Definitions
{
    /// <summary>
    /// <see cref="https://docs.microsoft.com/en-us/azure/governance/policy/concepts/definition-structure#type"/>
    /// </summary>
    public enum PolicyTypeEnum
    {
        /// <summary>
        /// These policy definitions are provided and maintained by system
        /// </summary>
        Builtin,
        /// <summary>
        /// All policy definitions created by customers have this value.
        /// </summary>
        Custom,
        /// <summary>
        /// Indicates a Regulatory Compliance policy 
        /// </summary>
        Static
    }
}
