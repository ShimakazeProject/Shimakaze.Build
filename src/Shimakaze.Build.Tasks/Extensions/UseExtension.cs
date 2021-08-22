namespace Shimakaze.Build.Tasks.Extensions;

internal static class UseExtension
{
    public static void Use<T>(this T t, Action<T> action)
        where T : IDisposable
    {
        using (t)
            action(t);
    }
}