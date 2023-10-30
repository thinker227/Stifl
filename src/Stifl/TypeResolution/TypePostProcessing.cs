using Stifl.Types;

namespace Stifl.TypeResolution;

internal static class TypePostProcessing
{
    /// <summary>
    /// Processes a set of annotations after constraint solving into proper types.
    /// </summary>
    /// <param name="annotations">The annotations to process.</param>
    /// <param name="scopes">The scopes of the annotated nodes.</param>
    /// <param name="symbols">The symbols to process.</param>
    public static TypeSet Process(
        AnnotationSet annotations,
        ScopeSet scopes,
        SymbolSet symbols)
    {
        var types = new Dictionary<AstOrSymbol, IType>();
        
        // Purify type variables into their substitutions.
        foreach (var (x, variable) in annotations)
        {
            var type = variable.Purify();
            
            // All unsolved type variables should have been solved by the previous step.
            // Otherwise, this is probably a bug.
            if (type is TypeVariable) throw new InvalidOperationException(
                $"Unexpected type variable {type} remaining after having tried " +
                "to solve all unsolved type variables after unification.");

            types[x] = type;
        }
        
        return types;
    }
}
