﻿namespace maskx.AzurePolicy.Extensions
{
    public static class StringExtensions
    {
        public static string GetRawString(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            return s.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }
    }
}
