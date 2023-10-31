using Stifl.Types;

namespace Stifl.TypeResolution;

/// <summary>
/// Generates sequential type variables.
/// </summary>
internal sealed class TypeVariableInator
{
    private int currentIndex = 0;

    /// <summary>
    /// Gets the next type variable.
    /// </summary>
    public TypeVariable Next() => new(currentIndex++);

    /// <summary>
    /// Returns a function for handling instantiations.
    /// </summary>
    public Func<TypeParameter, TypeVariable> Instantiation()
    {
        var vars = null as Dictionary<TypeParameter, TypeVariable>;

        return param =>
        {
            vars ??= [];
            return vars.GetOrAdd(param, Next);
        };
    }
}
