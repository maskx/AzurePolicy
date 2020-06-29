using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AzurePolicyTest
{
    [Collection("WebHost PolicyService")]
    [Trait("c", "Effect")]
    public class LogicalTest
    {
        private readonly PolicyServiceFixture fixture;
        public LogicalTest(PolicyServiceFixture fixture)
        {
            this.fixture = fixture;
        }
    }
}
