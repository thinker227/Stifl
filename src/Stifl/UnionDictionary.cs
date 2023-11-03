using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Stifl;

/// <summary>
/// A dictionary which acts as a union between two other dictionaries,
/// favoring the first of the two dictionaries.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
/// <param name="a">The first dictionary. Keys from this dictionary will be favored.</param>
/// <param name="b">The second dictionary.</param>
internal sealed class UnionDictionary<TKey, TValue>(
    IReadOnlyDictionary<TKey, TValue> a,
    IReadOnlyDictionary<TKey, TValue> b)
    : IReadOnlyDictionary<TKey, TValue>
{
    public TValue this[TKey key] =>
        a.TryGetValue(key, out var value)
            ? value
            : b[key];

    public IEnumerable<TKey> Keys => a.Keys.Concat(b.Keys);

    public IEnumerable<TValue> Values => a.Values.Concat(b.Values);

    public int Count => a.Count + b.Count;

    public bool ContainsKey(TKey key) => a.ContainsKey(key) || b.ContainsKey(key);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) =>
        a.TryGetValue(key, out value) || b.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
        a.Concat(b).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static partial class Utility
{
    /// <summary>
    /// Creates a <see cref="UnionDictionary{TKey, TValue}"/> from two dictionaries,
    /// the first of which being favored.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionaries.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionaries.</typeparam>
    /// <param name="a">The first dictionary. Keys from this dictionary will be favored.</param>
    /// <param name="b">The second dictionary.</param>
    public static UnionDictionary<TKey, TValue> Union<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> a,
        IReadOnlyDictionary<TKey, TValue> b) =>
        new(a, b);
}
