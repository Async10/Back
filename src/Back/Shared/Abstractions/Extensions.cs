namespace Back.Shared.Abstractions;

public static class Extensions
{
    public static IEnumerable<(int, T)> Enumerate<T>(this IEnumerable<T> items)
    {
        int i = 0;
        foreach (var item in items)
            yield return (i++, item);
    }
}