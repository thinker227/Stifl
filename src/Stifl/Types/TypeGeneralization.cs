namespace Stifl.Types;

/// <summary>
/// A generalization over one or multiple type parameters.
/// </summary>
/// <remarks>
/// A generalization may only appear as the top-level type of a binding.
/// </remarks>
public interface ITypeGeneralization : IType
{
    /// <summary>
    /// The type parameters for the type.
    /// </summary>
    IReadOnlySet<ITypeParameter> ForallTypes { get; }
    
    /// <summary>
    /// The type generalized over.
    /// </summary>
    IType Containing { get; }
}

/// <summary>
/// A mutable generalization over one or multiple type parameters.
/// </summary>
/// <param name="forallTypes">The type parameters for the type.</param>
/// <param name="containing">The type generalized over.</param>
internal sealed class TypeGeneralization(
    IType containing,
    IEnumerable<ITypeParameter>? forallTypes = null) : ITypeGeneralization, IEquatable<TypeGeneralization>
{
    public IType Containing { get; set; } = containing;

    public HashSet<ITypeParameter> ForallTypes { get; set; } = new(forallTypes ?? []);

    IReadOnlySet<ITypeParameter> ITypeGeneralization.ForallTypes => ForallTypes;
    
    public IType Substitute<T>(Func<T, bool> predicate, Func<T, IType> sub) where T : IType =>
        TypeExtensions.Sub(this, predicate, sub, x => new TypeGeneralization(
            x.Containing.Substitute(predicate, sub), x.ForallTypes));

    public IEnumerable<IType> Children() => [..ForallTypes, Containing];

    public bool Equals(TypeGeneralization? other) =>
        other is not null &&
        other.Containing.Equals(Containing) &&
        other.ForallTypes.SetEquals(ForallTypes);

    public override bool Equals(object? obj) =>
        Equals(obj as TypeGeneralization);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Containing);
        foreach (var type in ForallTypes) hashCode.Add(type);
        return hashCode.ToHashCode();
    }
    
    public override string ToString()
    {
        if (ForallTypes.Count == 0) return $"∀ {{}}. {Containing}";

        var types = string.Join(" ", ForallTypes.Select(x => x.ToString()));
        return $"∀ {{ {types} }}. {Containing}";
    }

    public static bool operator ==(TypeGeneralization a, TypeGeneralization b) =>
        a.Equals(b);

    public static bool operator !=(TypeGeneralization a, TypeGeneralization b) =>
        !a.Equals(b);
}
