using System.Collections;

namespace Shimakaze.Build.Tasks.Extensions;

internal static class EachExtension
{
    public static void Each<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (T item in source)
            action(item);
    }

    public static void Each<T>(this IEnumerable source, Action<T> action)
    {
        foreach (T item in source)
            action(item);
    }
    public static IEnumerable<TResult> Each<T, TResult>(this IEnumerable source, Func<T, TResult> func)
    {
        List<TResult> result = new();
        foreach (T item in source)
            result.Add(func(item));
        return result;
    }
}