using System.Collections;
using System.Text;
using Stifl.Types;

namespace Stifl.Interpret;

/// <summary>
/// A list value.
/// </summary>
/// <param name="type">The type of the list.</param>
/// <param name="eval">A function to evaluate the value.</param>
/// <remarks>Enumerating this type will evaluate it (but not its elements).</remarks>
public sealed class ListValue(ListType type, Func<(IValue, ListValue)?> eval)
    : Value<(IValue head, ListValue tail)?, ListType>(type, eval), IEnumerable<IValue>
{
    /// <summary>
    /// Creates an empty list.
    /// </summary>
    /// <param name="type">The type of the list.</param>
    public static ListValue Empty(ListType type) =>
        new(type, () => null);

    /// <summary>
    /// Creates a new <see cref="ListValue"/> from a list of values.
    /// </summary>
    /// <param name="type">The type of the list.</param>
    /// <param name="values">The list of values to create the tuple from.</param>
    /// <returns>The created list value,
    /// or <see langword="null"/> if the values in the list are not of the provided type.</returns>
    public static ListValue? FromList(ListType type, IEnumerable<IValue> values)
    {
        var current = new ListValue(type, () => null);

        foreach (var value in values)
        {
            if (!value.Type.Equals(type.Containing)) return null;

            var c = current;
            current = new(type, () => (value, c));
        }

        return current;
    }

    public IEnumerator<IValue> GetEnumerator()
    {
        var current = Eval();

        while (current is not null)
        {
            yield return current.Value.head;
            current = current.Value.tail.Eval();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected override string? DisplayValue((IValue head, ListValue tail)? value)
    {
        if (value is null) return "[]";

        var builder = new StringBuilder();
        builder.Append('[');

        var current = this;
        while (true)
        {
            if (!current.TryGetValue(out var val))
            {
                builder.Append("...");
                break;
            }

            if (val is not (var h, var t)) break;

            builder.Append(h);
            if (!t.TryGetValue(out var x) || x is not null)
            {
                builder.Append(", ");
            }

            current = t;
        }

        builder.Append(']');

        return builder.ToString();
    }
}
