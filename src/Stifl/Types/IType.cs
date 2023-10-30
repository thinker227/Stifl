namespace Stifl.Types;

/// <summary>
/// A semantic representation of a type.
/// </summary>
public interface IType
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
    IType Instantiate(Func<TypeParameter, TypeVariable> var);

    /// <summary>
    /// Gets the types contained within the type.
    /// </summary>
    IEnumerable<IType> ContainedTypes();
}
