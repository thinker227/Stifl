using System.Collections;
using Stifl.Types;

namespace Stifl.Interpret;

/// <summary>
/// A tuple value.
/// </summary>
/// <param name="type">The type of the tuple.</param>
/// <param name="eval">A function to evaluate the value.</param>
/// <remarks>Enumerating this type will evaluate it (but not its elements).</remarks>
public sealed class TupleValue(TupleType type, Func<ImmutableArray<IValue>> eval)
    : Value<ImmutableArray<IValue>, TupleType>(type, eval), IEnumerable<IValue>
{
    /// <summary>
    /// Creates a <see cref="TupleValue"/> from a list of values.
    /// </summary>
    /// <param name="values">The list of values to create the tuple from.</param>
    public static TupleValue FromList(IEnumerable<IValue> values)
    {
        var vals = values.ToImmutableArray();
        var types = vals.Select(val => val.Type).ToList();
        var type = new TupleType(types);
        return new(type, () => vals);
    }

    public IEnumerator<IValue> GetEnumerator() =>
        ((IEnumerable<IValue>)Eval()).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected override string? DisplayValue(ImmutableArray<IValue> value) =>
        $"({string.Join(", ", value)})";
}
