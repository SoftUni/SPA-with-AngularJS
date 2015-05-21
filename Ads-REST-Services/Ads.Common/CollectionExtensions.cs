namespace Ads.Common
{
    using System.Collections.Generic;

    public static class CollectionExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerable<T> collection)
        {
            return collection;
        }
    }
}
