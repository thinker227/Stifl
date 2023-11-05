
namespace Stifl.Types;

/// <summary>
/// A tuple type.
/// </summary>
/// <param name="Types">The types which the tuple consists of.</param>
public sealed record TupleType(IReadOnlyList<IType> Types) : IType
{
    public int Arity => Types.Count;
    
    public IType Purify() =>
        new TupleType(Types.Select(t => t.Purify()).ToList());

    public IType Instantiate(Func<TypeParameter, TypeVariable> var) =>
        new TupleType(Types.Select(t => t.Instantiate(var)).ToList());

    public IType ReplaceVars(Func<TypeVariable, IType> replace) =>
        new TupleType(Types.Select(t => t.ReplaceVars(replace)).ToList());

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
