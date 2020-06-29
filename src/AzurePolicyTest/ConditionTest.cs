using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AzurePolicyTest
{
    [Collection("WebHost PolicyService")]
    [Trait("c", "Condition")]
    public class ConditionTest
    {
        private readonly PolicyServiceFixture fixture;
        public ConditionTest(PolicyServiceFixture fixture)
        {
            this.fixture = fixture;
        }
    }
}
