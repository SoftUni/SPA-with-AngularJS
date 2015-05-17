namespace SocialNetwork.Common
{
    using System;
    using System.Linq;

    public static class QueryableExtensions
    {
        public static IQueryable<T> TakeLast<T>(this IQueryable<T> source, int n)
        {
            return source.Skip(Math.Max(0, source.Count() - n));
        }
    }
}
