namespace Stifl.Types;

/// <summary>
/// A temporary type variable for unification.
/// </summary>
/// <param name="Index">The unique index for the variable per AST.</param>
public sealed record TypeVariable(int Index) : IType
{
    private IType? substitution = null;

    /// <summary>
    /// Whether the variable has a substitution.
    /// </summary>
    public bool HasSustitution => substitution is not null;

    public IType Substitution => substitution switch
    {
        null => this,
        TypeVariable var => var.Substitution,
        _ => substitution,
    };

    /// <summary>
    /// Substitutes the variable for another type.
    /// </summary>
    /// <param name="type">The type to substitute the variable for.</param>
    /// <remarks>This method can only be call once.</remarks>
    public void Substitute(IType type)
    {
        if (ReferenceEquals(type, this))
            throw new InvalidOperationException(
                $"Cannot substitute type variable {this} for itself.");

        if (substitution is not null && !type.Equals(substitution))
            throw new InvalidOperationException(
                $"t{Index} has already been substituted with {substitution} " +
                $"and cannot be substituted with incompatible type {type}.");                

        substitution = type;
    }

    public IType Purify() => substitution?.Purify() ?? this;

    public IEnumerable<IType> ContainedTypes() =>
        substitution is not null
            ? [substitution]
            : [];

    public IType Instantiate(Func<TypeParameter, TypeVariable> var) => substitution?.Instantiate(var) ?? this;

    public override string ToString() =>
        substitution is null
            ? $"t{Index}"
            : substitution.ToString()!;
}
