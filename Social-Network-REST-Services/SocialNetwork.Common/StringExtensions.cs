namespace SocialNetwork.Common
{
    using System;
    using System.Collections.Generic;
    using System.Web.Script.Serialization;

    public static class StringExtensions
    {
        public static IDictionary<TK, TV> ToJson<TK, TV>(this string str)
        {
            var jsSerializer = new JavaScriptSerializer();

            return jsSerializer.Deserialize<Dictionary<TK, TV>>(str);
        }

        public static IDictionary<string, string> ToJson(this string str)
        {
            var jsSerializer = new JavaScriptSerializer();

            return jsSerializer.Deserialize<Dictionary<string, string>>(str);
        }

        public static bool ContainsCaseInsensitive(this string str, string searchTerm)
        {
            return str.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
