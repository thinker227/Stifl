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
        IEnumerable<Ast.Decl.Binding> bindings,
        AnnotationSet annotations,
        SymbolSet symbols)
    {
        var types = new Dictionary<AstOrSymbol, IType>();

        foreach (var binding in bindings)
        {
            // This is a more manual way of purifying the type variable for the
            // binding without also removing the generalization builder. The
            // containing type isn't needed here, only the forall types, so
            // the containing type doesn't need to be purified.
            var generalization = (GeneralizationBuilder)annotations[binding].Substitution;
            
            var processor = new BindingProcessor(generalization, annotations);

            // Looping through nodes and symbols is done like this because there's no
            // good way of associating a node (and symbol) to its parent binding.
            var nodes = binding.DescendantsAndSelf<Ast>(n => annotations.ContainsKey(n));
            foreach (var node in nodes)
            {
                types[node] = processor.Node(node);

                if (symbols.TryGetValue(node, out var symbol))
                    types[AstOrSymbol.FromT1(symbol)] = processor.Symbol(symbol);
            }
        }
        
        return types;
    }
}

file sealed class BindingProcessor(
    GeneralizationBuilder generalization,
    AnnotationSet annotations)
{
    private readonly Dictionary<TypeVariable, InferredTypeParameter> parameters = [];

    public IType Process(AstOrSymbol x) => annotations[x]
        .Purify()
        .Substitute<TypeVariable>(var =>
            parameters.GetOrAdd(var, () =>
            {
                var param = new InferredTypeParameter();
                generalization.ForallTypes.Add(param);
                return param;
            }));

    public IType Node(Ast node) => Process(node);

    public IType Symbol(ISymbol symbol) => Process(AstOrSymbol.FromT1(symbol));
}
