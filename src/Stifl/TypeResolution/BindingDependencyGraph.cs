using Pidgin;

namespace Stifl.TypeResolution;

internal static class BindingDependencyGraph
{
    /// <summary>
    /// Finds the dependencies between a set of bindings.
    /// </summary>
    /// <param name="bindings">The bindings to find the dependencies between.</param>
    /// <param name="scopes">The scopes of the bindings.</param>
    /// <returns>A dictionary of bindings and their dependencies.</returns>
    public static IReadOnlyDictionary<Ast.Decl.Binding, IReadOnlyCollection<Ast.Decl.Binding>> FindDependencies(
        IEnumerable<Ast.Decl.Binding> bindings,
        ScopeSet scopes)
    {
        var dependencies = new Dictionary<Ast.Decl.Binding, IReadOnlyCollection<Ast.Decl.Binding>>();

        foreach (var binding in bindings)
        {
            var visitor = new DependencyVisitor(scopes);
            visitor.VisitNode(binding);
            dependencies[binding] = visitor.dependencies;
        }

        return dependencies;
    }
}

file sealed class DependencyVisitor(ScopeSet scopes) : AstVisitor<Unit>
{
    public readonly List<Ast.Decl.Binding> dependencies = [];

    protected override Unit Default => Unit.Value;

    public override Unit VisitIdentifierExpr(Ast.Expr.Identifier node)
    {
        if (scopes[node].Lookup(node.Name) is Binding binding)
            dependencies.Add(binding.Declaration);

        return Default;
    }
}
