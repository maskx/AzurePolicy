namespace maskx.AzurePolicy.Functions
{
    public static class ContextKeys
    {
        public const string POLICY_CONTEXT = "policy_context";

        // https://docs.microsoft.com/en-us/azure/governance/policy/concepts/definition-structure#count-examples
        // field function need this to return count
        public const string COUNT_ELEMENT = "count_element";

        public const string COUNT_FIELD = "count_field";
        public const string INITIATIVE_PARAMERTERS = "initiative_paramerters";
    }
}