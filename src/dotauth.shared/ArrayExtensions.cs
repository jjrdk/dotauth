namespace DotAuth.Shared;

using System.Collections.Generic;
using System.Linq;

internal static class ArrayExtensions
{
    public static T[] Add<T>(this IEnumerable<T> array, params T[] items)
    {
        return array.Concat(items).ToArray();
    }
    
    public static T[] Add<T>(this IEnumerable<T> array, IEnumerable<T> items)
    {
        return array.Concat(items).ToArray();
    }
    
    public static T[] Remove<T>(this T[] array, params T[] items)
    {
        return array.Except(items).ToArray();
    }
}