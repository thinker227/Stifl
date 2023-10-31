using static Stifl.Ast;

namespace Stifl;

public static class AstChildren
{
    private static IEnumerable<T> EmptyIfNull<T>(T? value) =>
        value is not null
            ? [value]
            : [];

    /// <summary>
    /// Gets the children of a node.
    /// </summary>
    /// <param name="node">The node to get the children of.</param>
    public static IEnumerable<Ast> Children(this Ast node) => node switch
    {
        Unit x => [..x.Decls],

        Decl.Binding x => [..EmptyIfNull(x.AnnotatedType), x.Expression],

        Expr.Unit => [],
        Expr.UndefinedLiteral => [],
        Expr.BoolLiteral => [],
        Expr.IntLiteral => [],
        Expr.Identifier => [],
        Expr.Func x => [..EmptyIfNull(x.AnnotatedType), x.Body],
        Expr.If x => [x.Condition, x.IfTrue, x.IfFalse],
        Expr.Call x => [x.Function, x.Argument],
        Expr.Let x => [..EmptyIfNull(x.AnnotatedType), x.Value, x.Expression],
        Expr.Annotated x => [x.Expression, x.Annotation],

        AstType.Unit => [],
        AstType.Int => [],
        AstType.Bool => [],
        AstType.Func x => [x.Parameter, x.Return],
        AstType.Var => [],
        
        _ => throw new UnreachableException($"Cannot get children of node type {node.GetType()}."),
    };

    /// <summary>
    /// Gets the children of a node with the node prepended to the sequence.
    /// </summary>
    /// <param name="node">The node to get the children of.</param>
    public static IEnumerable<Ast> ChildrenAndSelf(this Ast node) =>
        node.Children().Prepend(node);

    /// <summary>
    /// Gets the descendants of a node.
    /// </summary>
    /// <param name="node">The node to get the descendants of.</param>
    public static IEnumerable<Ast> Descendants(this Ast node) =>
        node.Children().SelectMany(DescendantsAndSelf);

    /// <summary>
    /// Gets the descendants of a node with the node prepended to the sequence.
    /// </summary>
    /// <param name="node">The node to get the descendants of.</param>
    public static IEnumerable<Ast> DescendantsAndSelf(this Ast node) =>
        node.Descendants().Prepend(node);

    /// <summary>
    /// Gets the descendants of a node.
    /// </summary>
    /// <param name="node">The node to get the descendants of.</param>
    /// <param name="filter">A function to filer nodes.
    /// If returns <see langword="false"/> for a node,
    /// the node will not be returned
    /// and none of its children will be visited.</param>
    public static IEnumerable<Ast> Descendants(this Ast node, Func<Ast, bool> filter) =>
        node.Children().SelectMany(n => n.DescendantsAndSelf(filter));

    /// <summary>
    /// Gets the descendants of a node with the node prepended to the sequence.
    /// </summary>
    /// <param name="node">The node to get the descendants of.</param>
    /// <param name="filter">A function to filer nodes.
    /// If returns <see langword="false"/> for a node,
    /// the node will not be returned
    /// and none of its children will be visited.</param>
    public static IEnumerable<Ast> DescendantsAndSelf(this Ast node, Func<Ast, bool> filter)
    {
        if (!filter(node)) return [];

        return node.Descendants(filter).Prepend(node);
    }
}
