using static Stifl.Ast;
using Stifl.Types;

namespace Stifl.TypeResolution;

internal static class TypeAnnotate
{
    /// <summary>
    /// Annotates the types of an AST node and its descendants and a set of symbols.
    /// </summary>
    /// <param name="node">The node to annotate.</param>
    /// <param name="symbols">The scopes of the nodes.</param>
    public static AnnotationSet Annotate(
        IEnumerable<Decl.Binding> bindings,
        SymbolSet symbols,
        TypeVariableInator variableInator)
    {
        var visitor = new TypeAnnotateVisitor(symbols, variableInator);
        foreach (var binding in bindings) visitor.VisitNode(binding);
        return visitor.annotations;
    }
}

file sealed class TypeAnnotateVisitor(SymbolSet symbols, TypeVariableInator variableInator) : AstVisitor
{
    public int index = 0;
    public readonly Dictionary<AstOrSymbol, TypeVariable> annotations = new(
        new OneOfEqualityComparer<Ast, ISymbol>(
            ReferenceEqualityComparer.Instance,
            ReferenceEqualityComparer.Instance));

    private void Annotate(Ast node, TypeVariable annotation) =>
        annotations[AstOrSymbol.FromT0(node)] = annotation;

    private void Annotate(ISymbol symbol, TypeVariable annotation) =>
        annotations[AstOrSymbol.FromT1(symbol)] = annotation;

    protected override void BeforeVisit(Ast node)
    {
        if (node is not (Expr or Decl.Binding)) return;

        Annotate(node, variableInator.Next());
    }

    public override void VisitBindingDecl(Decl.Binding node)
    {
        var symbol = symbols[node];
        Annotate(symbol, annotations[node]);

        VisitNode(node.AnnotatedType);
        VisitNode(node.Expression);
    }

    public override void VisitFuncExpr(Expr.Func node)
    {
        var symbol = symbols[node];
        Annotate(symbol, variableInator.Next());

        VisitNode(node.AnnotatedType);
        VisitNode(node.Body);
    }

    public override void VisitLetExpr(Expr.Let node)
    {
        var symbol = symbols[node];

        VisitNode(node.AnnotatedType);
        VisitNode(node.Expression);
        VisitNode(node.Value);

        // Has to visit node.Value before annotating the symbol.
        Annotate(symbol, annotations[node.Value]);
    }
}
