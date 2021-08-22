namespace Shimakaze.Build.Tasks.Extensions;

internal static class TaskExtension
{
    public static T WaitSync<T>(this Task<T> task)
    {
        task.RunSynchronously();
        return task.Result;
    }
}