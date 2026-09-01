namespace DotAuth.Shared;

using System.Collections.Generic;
using System.Linq;

internal static class ArrayExtensions
{
    public static T[] Add<T>(this IEnumerable<T> source, params T[] items)
    {
        var arr = source as T[] ?? [.. source];
        var result = new T[arr.Length + items.Length];
        arr.CopyTo(result, 0);
        items.CopyTo(result, arr.Length);
        return result;
    }

    public static T[] Add<T>(this IEnumerable<T> array, IEnumerable<T> items)
    {
        return array.Add([.. items]);
    }

    public static T[] Remove<T>(this T[] array, params T[] items)
    {
        return [.. array.Except(items)];
    }
}
