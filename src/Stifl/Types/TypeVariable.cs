namespace Stifl.Types;

/// <summary>
/// A temporary type variable for unification.
/// </summary>
/// <param name="Index">The unique index for the variable per AST.</param>
public sealed class TypeVariable(int Index) : IType
{
    private IType? substitution = null;

    /// <summary>
    /// Whether the variable has a substitution.
    /// </summary>
    public bool HasSustitution => substitution is not null;

    public IType Substitution => substitution switch
    {
        null => this,
        // Pruning
        TypeVariable var => substitution = var.Substitution,
        _ => substitution,
    };

    /// <summary>
    /// Substitutes the variable for another type.
    /// </summary>
    /// <param name="type">The type to substitute the variable for.</param>
    /// <remarks>This method can only be call once.</remarks>
    public void Substitute(IType type)
    {
        if (ReferenceEquals(type, this)) return;

        if (substitution is not null && !type.Equals(substitution))
            throw new InvalidOperationException(
                $"t{Index} has already been substituted with {substitution} " +
                $"and cannot be substituted with incompatible type {type}.");

        substitution = type;
    }
    
    public IType Substitute<T>(Func<T, bool> predicate, Func<T, IType> sub) where T : IType =>
        TypeExtensions.Sub(this, predicate, sub, x =>
            x.HasSustitution
                ? x.Substitution.Substitute(predicate, sub)
                : x);

    public IEnumerable<IType> Children() =>
        substitution is not null
            ? [substitution]
            : [];

    public override string ToString() =>
        substitution is null
            ? $"t{Index}"
            : substitution.ToString()!;
}
