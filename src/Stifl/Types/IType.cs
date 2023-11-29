namespace Stifl.Types;

/// <summary>
/// A semantic representation of a type.
/// </summary>
public interface IType : INode<IType>
{
    /// <summary>
    /// Substitutes specific types matching a predicate inside a type with another type. 
    /// </summary>
    /// <param name="predicate">The predicate which determines whether a type should be substituted.</param>
    /// <param name="sub">A function which substitutes one type for another.</param>
    /// <typeparam name="T">The kind of type which should be substituted.</typeparam>
    /// <returns>The original type with containing types matching the predicate substituted.</returns>
    IType Substitute<T>(Func<T, bool> predicate, Func<T, IType> sub) where T : IType;
}

public static class TypeExtensions
{
    /// <summary>
    /// Substitutes specific types matching a predicate inside a type with another type. 
    /// </summary>
    /// <param name="type">The type to substitute within.</param>
    /// <param name="sub">A function which substitutes one type for another.</param>
    /// <typeparam name="T">The kind of type which should be substituted.</typeparam>
    /// <returns>The original type with containing types matching the predicate substituted.</returns>
    public static IType Substitute<T>(this IType type, Func<T, IType> sub)
        where T : IType =>
        type.Substitute(_ => true, sub);
    
    /// <summary>
    /// Tries to remove all type variables from the type.
    /// </summary>
    /// <param name="type">The type to purify.</param>
    /// <remarks>
    /// This method does not guarantee all type variables are removed,
    /// only type variables with substitutions.
    /// </remarks>
    public static IType Purify(this IType type) => type
        .Substitute<TypeVariable>(x => x.Substitution)
        .Substitute<GeneralizationBuilder>(x => new TypeGeneralization(x.ForallTypes, x.Containing));

    /// <summary>
    /// Removes generalizations and replaces all type parameters in the type with new type variables.
    /// </summary>
    /// <param name="type">The type to instantiate.</param>
    /// <param name="var">A function to generate new type variables from type parameters.</param>
    public static IType Instantiate(this IType type, Func<ITypeParameter, TypeVariable> var) => type
            .Substitute<IType>(
                x => x is TypeGeneralization or GeneralizationBuilder,
                x => x switch
                {
                    TypeGeneralization g => g.Containing,
                    GeneralizationBuilder g => g.Containing,
                    _ => throw new UnreachableException()
                })
            .Substitute(var);
    
    /// <summary>
    /// Helper for implementing <see cref="IType.Substitute{T}"/>.
    /// </summary>
    /// <param name="this">The <see langword="this"/> value.</param>
    /// <param name="predicate">The predicate passed to <see cref="IType.Substitute{T}"/>.</param>
    /// <param name="sub">The substitution function passed to <see cref="IType.Substitute{T}"/>.</param>
    /// <param name="subContainingTypes">A function which substitutes containing types.</param>
    /// <typeparam name="T">The type kind passed to <see cref="IType.Substitute{T}"/>.</typeparam>
    /// <typeparam name="TType">The self type.</typeparam>
    internal static IType Sub<T, TType>(
        TType @this,
        Func<T, bool> predicate,
        Func<T, IType> sub,
        Func<TType, IType> subContainingTypes)
        where T : IType
        where TType : IType
    {
        if (@this is not T x || !predicate(x)) return subContainingTypes(@this);
        
        // Avoid infinite recursion.
        var s = sub(x);
        return s.Equals(x)
            ? s
            : s.Substitute(predicate, sub);
    }
}
