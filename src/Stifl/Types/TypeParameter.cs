namespace Stifl.Types;

/// <summary>
/// A type parameter.
/// </summary>
/// <param name="name">The name of the type parameter.</param>
public sealed class TypeParameter(string name) : IType, ISymbol
{
    string ISymbol.Name => $"'{name}";

    public IType Purify() => this;
    
    public IType Instantiate(Func<TypeParameter, TypeVariable> var) => var(this);

    public IEnumerable<IType> ContainedTypes() => [];

    public override string ToString() => $"'{name}";

    // TODO: Distinguish type parameters based on scope?
}
