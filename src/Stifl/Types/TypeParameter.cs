namespace Stifl.Types;

/// <summary>
/// A type parameter in a <see cref="ITypeGeneralization"/>.
/// </summary>
public interface ITypeParameter : IType;

/// <summary>
/// An type parameter which is created during type inference.
/// </summary>
public sealed class InferredTypeParameter : ITypeParameter
{
#if DEBUG
    private static int currentIndex = 0;

    private readonly int index = currentIndex++;
#endif
    
    public IType Substitute<T>(Func<T, bool> predicate, Func<T, IType> sub) where T : IType =>
        TypeExtensions.Sub(this, predicate, sub, x => x);

    public IEnumerable<IType> Children() => [];

#if DEBUG
    public override string ToString() => $"'T{index}";
#endif
}

/// <summary>
/// An explicitly declared type parameter.
/// </summary>
/// <param name="name">The name of the type parameter.</param>
public sealed class TypeParameter(string name) : ITypeParameter, ISymbol
{
    public string Name => $"'{name}";

    /// <summary>
    /// The name of the type parameter without the leading <c>'</c>.
    /// </summary>
    public string OrdinalName => name;
    
    public IType Substitute<T>(Func<T, bool> predicate, Func<T, IType> sub) where T : IType =>
        TypeExtensions.Sub(this, predicate, sub, x => new TypeParameter(name));

    public IEnumerable<IType> Children() => [];

    public override string ToString() => Name;
}
