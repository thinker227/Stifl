using System.Collections;
using OneOf;

namespace Stifl;

/// <summary>
/// Miscellaneous utilities.
/// </summary>
internal static partial class Utility
{
    /// <summary>
    /// Turns a <see cref="Pidgin.Maybe{T}"/> into a nullable value.
    /// </summary>
    /// <typeparam name="T">The type inside the maybe.</typeparam>
    /// <param name="maybe">The source maybe.</param>
    public static T? Nullable<T>(this Pidgin.Maybe<T> maybe) where T : class =>
        maybe.HasValue
            ? maybe.Value
            : null;

    /// <summary>
    /// Enumerates an enumerable and discards the result.
    /// This is intended for impure enumerables.
    /// </summary>
    public static void Enumerate<T>(this IEnumerable<T> xs)
    {
        foreach (var _ in xs) {}
    }

    /// <summary>
    /// Gets a value from a dictionary or adds it if it does not exist.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dict">The dictionary to look up values in.</param>
    /// <param name="key">The key to try find the value using.</param>
    /// <param name="getValue">A function to create a new value if it does not exist.</param>
    /// <returns>The existing or the newly added value.</returns>
    public static TValue GetOrAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> dict,
        TKey key,
        Func<TValue> getValue)
    {
        if (!dict.TryGetValue(key, out var value))
        {
            value = getValue();
            dict.Add(key, value);
        }

        return value;
    }

    /// <summary>
    /// Zips each element of a sequence with its next element.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="xs">The source sequence.</param>
    public static IEnumerable<(T, T)> WithNext<T>(this IEnumerable<T> xs)
    {
        var first = true;
        var prev = default(T);

        foreach (var x in xs)
        {
            if (!first) yield return (prev!, x);

            first = false;
            prev = x;
        }
    }
}
