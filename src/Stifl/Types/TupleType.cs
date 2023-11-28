
namespace Stifl.Types;

/// <summary>
/// A tuple type.
/// </summary>
/// <param name="Types">The types which the tuple consists of.</param>
public sealed record TupleType(IReadOnlyList<IType> Types) : IType
{
    public int Arity => Types.Count;
    
    public IType Substitute<T>(Func<T, bool> predicate, Func<T, IType> sub) where T : IType =>
        TypeExtensions.Sub(this, predicate, sub, x => new TupleType(
            x.Types.Select(t => t.Substitute(predicate, sub)).ToList()));

    public IEnumerable<IType> Children() => Types;

    public bool Equals(TupleType? other) =>
        other?.Types.SequenceEqual(Types) ?? false;

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        foreach (var t in Types) hashCode.Add(t);
        return hashCode.ToHashCode();
    }

    public override string ToString()
    {
        var types = string.Join(", ", Types.Select(t => t.ToString()));
        return $"({types})";
    }
}
