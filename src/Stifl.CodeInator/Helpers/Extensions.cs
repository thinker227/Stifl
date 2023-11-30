namespace Stifl.CodeInator.Helpers;

internal static class Extensions
{
    public static IEnumerable<(T, bool)> HasNext<T>(this IEnumerable<T> xs)
    {
        var hasElem = false;
        var elem = default(T);

        foreach (var x in xs)
        {
            if (hasElem) yield return (elem!, true);
            
            hasElem = true;
            elem = x;
        }

        if (hasElem) yield return (elem!, false);
    }
}
