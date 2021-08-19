namespace Shimakaze.MSBuild.Extensions;

internal static class EachExtension{
    public static void Each<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }
}