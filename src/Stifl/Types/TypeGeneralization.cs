namespace Stifl.Types;

/// <summary>
/// A generalization over one or multiple type parameters.
/// </summary>
/// <remarks>
/// A generalization may only appear as the top-level type of a binding.
/// </remarks>
/// <param name="ForallTypes">The type parameters for the type.</param>
/// <param name="Containing">The type generalized over.</param>
public sealed record TypeGeneralization(
    IReadOnlySet<ITypeParameter> ForallTypes,
    IType Containing) : IType
{
    public IType Purify() => new TypeGeneralization(
        ForallTypes,
        Containing.Purify());
    
    // Instantiation should completely remove the generalization.
    public IType Instantiate(Func<ITypeParameter, TypeVariable> var) => Containing.Instantiate(var);

    public IType ReplaceVars(Func<TypeVariable, IType> replace) =>
        new TypeGeneralization(ForallTypes, Containing.ReplaceVars(replace));

    public IEnumerable<IType> Children() => [..ForallTypes, Containing];

    public override string ToString()
    {
        if (ForallTypes.Count == 0) return $"∀ {{}}. {Containing}";

        var types = string.Join(" ", ForallTypes.Select(x => x.ToString()));
        return $"∀ {{ {types} }}. {Containing}";
    }
}

/// <summary>
/// A mutable builder for a <see cref="TypeGeneralization"/>.
/// </summary>
/// <param name="Containing">The type generalized over.</param>
internal sealed class GeneralizationBuilder(
    IType containing,
    IEnumerable<ITypeParameter>? forallTypes = null)
    : IType
{
    /// <summary>
    /// The type parameters for the type.
    /// </summary>
    public HashSet<ITypeParameter> ForallTypes { get; } = forallTypes?.ToHashSet() ?? [];

    /// <summary>
    /// The type generalized over.
    /// </summary>
    public IType Containing { get; set; } = containing;

    public IType Purify() => new TypeGeneralization(ForallTypes, Containing.Purify());

    // A generalization builder should never be present at the time an instantiation would occur.
    IType IType.Instantiate(Func<ITypeParameter, TypeVariable> var) =>
        throw new InvalidOperationException(
            $"Unexpected type generalization builder {this} left over during instantiation.");

    public IType ReplaceVars(Func<TypeVariable, IType> replace) =>
        new GeneralizationBuilder(Containing.ReplaceVars(replace));

    public IEnumerable<IType> Children() => [..ForallTypes, Containing];

    public override string ToString()
    {
        if (ForallTypes.Count == 0) return $"∀ builder {{}}. {Containing}";

        var types = string.Join(" ", ForallTypes.Select(x => x.ToString()));
        return $"∀ builder {{ {types} }}. {Containing}";
    }
}
