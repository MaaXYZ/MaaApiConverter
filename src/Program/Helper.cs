namespace MaaApiConverter;

internal static class Helper
{
    public static T Dump<T>(this T result, params Action<T>[] actions)
    {
        foreach (var action in actions)
            action.Invoke(result);
        return result;
    }
}
