using System;

namespace Simple.Owin.Helpers
{
    internal static class UrlHelper
    {
        public static string Decode(string value) {
            value = value.Replace("+", " ");
            return Uri.UnescapeDataString(value);
        }

        public static string Encode(string value) {
            return Uri.EscapeDataString(value);
        }
    }
}