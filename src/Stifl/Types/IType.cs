namespace Stifl.Types;

/// <summary>
/// A semantic representation of a type.
/// </summary>
public interface IType : INode<IType>
{
    /// <summary>
    /// Tries to remove all type variables from the type.
    /// </summary>
    /// <remarks>
    /// This method does not guarantee all type variables are removed,
    /// only type variables with substitutions.
    /// </remarks>
    IType Purify();

    /// <summary>
    /// Removes generalizations and replaces all type parameters in the type with new type variables.
    /// </summary>
    /// <param name="var">A function to generate new type variables from type parameters.</param>
    IType Instantiate(Func<ITypeParameter, TypeVariable> var);

    /// <summary>
    /// Replaces unsolved type variables with another type.
    /// </summary>
    /// <param name="replace">A function to replace type variables with a new type.</param>
    IType ReplaceVars(Func<TypeVariable, IType> replace);
}
