using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AzurePolicyTest
{
    [Collection("WebHost PolicyService")]
    [Trait("c", "Effect")]
    public class CountTest
    {
        private readonly PolicyServiceFixture fixture;
        public CountTest(PolicyServiceFixture fixture)
        {
            this.fixture = fixture;
        }
    }
}
