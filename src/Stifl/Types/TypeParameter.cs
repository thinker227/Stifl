namespace Stifl.Types;

/// <summary>
/// A synthesized type parameter.
/// </summary>
public class TypeParameter : IType
{
    public IType Purify() => this;
    
    public IType Instantiate(Func<TypeParameter, TypeVariable> var) => var(this);

    public IType ReplaceVars(Func<TypeVariable, IType> replace) => this;

    public IEnumerable<IType> Children() => [];
}

/// <summary>
/// An explicitly declared type parameter.
/// </summary>
/// <param name="name">The name of the type parameter.</param>
public sealed class DeclaredTypeParameter(string name) : TypeParameter, ISymbol
{
    public string Name => $"'{name}";

    public override string ToString() => Name;
}
