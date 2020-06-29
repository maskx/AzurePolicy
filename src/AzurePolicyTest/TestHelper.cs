using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzurePolicyTest
{
    public static class TestHelper
    {
        public static IConfigurationRoot Configuration { get; private set; }
        public static string SubscriptionId = "C1FA36C2-4D58-45E8-9C51-498FADB4D8BF";
        public static string ResourceGroup = "ResourceGroup1";
        public static string CreateByUserId = "bob@163.com";
        static TestHelper()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets("A2865D9C-44FA-42CD-A9F6-278CDD99B107")
                .Build();
        }
        public static string GetJsonFileContent(string filename)
        {
            string s = Path.Combine(AppContext.BaseDirectory, $"{filename}.json");
            return File.ReadAllText(s);
        }
    }
}
