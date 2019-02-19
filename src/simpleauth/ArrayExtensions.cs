namespace SimpleAuth
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class ArrayExtensions
    {
        public static T[] Add<T>(this T[] array, params T[] items)
        {
            return items == null ? array : array.Concat(items).ToArray();
        }

        public static T[] Add<T>(this T[] array, IEnumerable<T> items)
        {
            return items == null ? array : array.Concat(items).ToArray();
        }

        public static T[] Remove<T>(this T[] array, params T[] items)
        {
            return items == null ? array : array.Except(items).ToArray();
        }
    }
}