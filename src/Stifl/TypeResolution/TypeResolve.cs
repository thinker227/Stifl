using Stifl.Types;

using Stifl.TypeResolution;
using Stifl.Graphs;

namespace Stifl;

/// <summary>
/// Methods for resolving types.
/// </summary>
public static class TypeResolve
{
    /// <summary>
    /// Resolves the types of a root node and a set of scopes and symbols.
    /// </summary>
    /// <param name="root">The root node.</param>
    /// <param name="scopes">The scopes of the AST.</param>
    /// <param name="symbols">The symbols in the AST.</param>
    /// <returns>A set of types for each AST node and applicable symbol.</returns>
    public static TypeSet ResolveTypes(Ast.Unit root, ScopeSet scopes, SymbolSet symbols)
    {
        var bindings = root.Decls.OfType<Ast.Decl.Binding>().ToList();
        var bindingGroups = GetBindingGroups(bindings, scopes);

        // This dictionary is used to gather all built-up types through type resolution.
        var types = new Dictionary<AstOrSymbol, IType>(
            new OneOfEqualityComparer<Ast, ISymbol>(
                ReferenceEqualityComparer.Instance,
                ReferenceEqualityComparer.Instance));

        // Visit binding groups in order.
        foreach (var group in bindingGroups)
        {
            var variableInator = new TypeVariableInator();
            var annotations = TypeAnnotate.Annotate(group, symbols, variableInator);

            // Add the annotations to the types.
            foreach (var (k, v) in annotations) types[k] = v;

            var constraints = ConstraintGeneration.GenerateConstraints(group, types, scopes, variableInator);
            ConstraintSolver.Solve(constraints);

            var bindingTypes = TypePostProcessing.Process(annotations, scopes, symbols);
        
            // Merge solved types with types.
            // Overrides previous annotations.
            foreach (var (k, v) in bindingTypes) types[k] = v;
        }

        return types;
    }

    /// <summary>
    /// Gets a collection of groups of bindings which have to be resolved together.
    /// </summary>
    /// <param name="bindings">The bindings to group.</param>
    /// <param name="scopes">The scopes of the bindings.</param>
    private static List<List<Ast.Decl.Binding>> GetBindingGroups(
        IEnumerable<Ast.Decl.Binding> bindings,
        ScopeSet scopes)
    {
        // Find the dependencies of each binding.
        var dependencies = BindingDependencyGraph.FindDependencies(bindings, scopes);
        // Construct a graph of bindings.
        var graph = Graph.Create(bindings, binding => dependencies[binding]);
        // Generate strongly connected components from the graph.
        var components = Component.Create(graph);
        // Generate a directed acyclic graph from the components.
        var dag = Component.GenerateAcyclicGraph(components);
        // Find a path through the acyclic graph.
        var path = Graph.GenerateAcyclicPath(dag);

        // Return the path mapped back to the bindings.
        return path
            .Select(x => x.Value.Nodes
                .Select(x => x.Value)
                .ToList())
            .ToList();
    }
}
