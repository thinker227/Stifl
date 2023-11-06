namespace Stifl;

public abstract partial record Ast : INode<Ast>
{
    private static IEnumerable<T> EmptyIfNull<T>(T? value) =>
        value is not null
            ? [value]
            : [];

    public IEnumerable<Ast> Children() => this switch
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
        Expr.Tuple x => [..x.Values],
        Expr.List x => [..x.Values],
        Expr.Annotated x => [x.Expression, x.Annotation],

        AstType.Unit => [],
        AstType.Int => [],
        AstType.Bool => [],
        AstType.Func x => [x.Parameter, x.Return],
        AstType.Tuple x => [..x.Types],
        AstType.List x => [x.Containing],
        AstType.Var => [],
        
        _ => throw new UnreachableException($"Cannot get children of node type {GetType()}."),
    };       
}
