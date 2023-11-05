using static Stifl.Ast;

namespace Stifl;

/// <summary>
/// Visits AST nodes.
/// </summary>
/// <typeparam name="T">The type which the visitor returns.</typeparam>
public abstract class AstVisitor<T>
{
    /// <summary>
    /// The default value of a visit.
    /// </summary>
    protected abstract T Default { get; }

    /// <summary>
    /// Called before a node is visited.
    /// </summary>
    /// <param name="node">The node being visited.</param>
    protected virtual void BeforeVisit(Ast node) {}

    /// <summary>
    /// Called after a node has been visited.
    /// </summary>
    /// <param name="node">The node being visited.</param>
    protected virtual void AfterVisit(Ast node) {}

    /// <summary>
    /// Filters nodes which should be visited.
    /// </summary>
    /// <param name="node">The node to filter.</param>
    /// <returns>Whether the node should be visited.</returns>
    protected virtual bool Filter(Ast node) => true;

    /// <summary>
    /// Visits many nodes and returns a lazy sequence of return values.
    /// </summary>
    /// <typeparam name="TNode">The type of the nodes to visit.</typeparam>
    /// <param name="nodes">The nodes to visit.</param>
    public IEnumerable<T> VisitMany<TNode>(IEnumerable<TNode> nodes)
        where TNode : Ast =>
        nodes.Select(VisitNode);

    /// <summary>
    /// Visits a node if it is not null.
    /// </summary>
    /// <param name="node">The node to visit.</param>
    /// <returns>The return value of the visit,
    /// or <see cref="Default"/> if the node is null.</returns>
    public T VisitNodeOrNull(Ast? node) =>
        node is not null
            ? VisitNode(node)
            : Default;

    /// <summary>
    /// Visits a node.
    /// </summary>
    /// <param name="node">The node to visit.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitNode(Ast node)
    {
        if (!Filter(node)) return Default;

        BeforeVisit(node);
        var x = VisitNodeCore(node);
        AfterVisit(node);

        return x;
    }

    private T VisitNodeCore(Ast node) => node switch
    {
        Unit x => VisitUnit(x),
        Decl x => VisitDecl(x),
        Expr x => VisitExpr(x),
        AstType x => VisitType(x),
        _ => throw new UnreachableException(),
    };

    public virtual T VisitUnit(Unit node)
    {
        VisitMany(node.Decls).Enumerate();
        return Default;
    }

    public virtual T VisitDecl(Decl node) => node switch
    {
        Decl.Binding x => VisitBindingDecl(x),
        _ => throw new UnreachableException(),
    };

    public virtual T VisitBindingDecl(Decl.Binding node)
    {
        VisitNodeOrNull(node.AnnotatedType);
        VisitNode(node.Expression);
        return Default;
    }

    public virtual T VisitExpr(Expr node) => node switch
    {
        Expr.Unit x => VisitUnitExpr(x),
        Expr.UndefinedLiteral x => VisitUndefinedLiteralExpr(x),
        Expr.BoolLiteral x => VisitBoolLiteralExpr(x),
        Expr.IntLiteral x => VisitIntLiteralExpr(x),
        Expr.Identifier x => VisitIdentifierExpr(x),
        Expr.Func x => VisitFuncExpr(x),
        Expr.If x => VisitIfExpr(x),
        Expr.Call x => VisitCallExpr(x),
        Expr.Let x => VisitLetExpr(x),
        Expr.Tuple x => VisitTupleExpr(x),
        Expr.List x => VisitListExpr(x),
        Expr.Annotated x => VisitAnnotatedExpr(x),
        _ => throw new UnreachableException(),
    };

    public virtual T VisitUnitExpr(Expr.Unit node) => Default;

    public virtual T VisitUndefinedLiteralExpr(Expr.UndefinedLiteral node) => Default;

    public virtual T VisitBoolLiteralExpr(Expr.BoolLiteral node) => Default;

    public virtual T VisitIntLiteralExpr(Expr.IntLiteral node) => Default;

    public virtual T VisitIdentifierExpr(Expr.Identifier node) => Default;

    public virtual T VisitFuncExpr(Expr.Func node)
    {
        VisitNodeOrNull(node.AnnotatedType);
        VisitNode(node.Body);
        return Default;
    }

    public virtual T VisitIfExpr(Expr.If node)
    {
        VisitNode(node.Condition);
        VisitNode(node.IfTrue);
        VisitNode(node.IfFalse);
        return Default;
    }

    public virtual T VisitCallExpr(Expr.Call node)
    {
        VisitNode(node.Function);
        VisitNode(node.Argument);
        return Default;
    }

    public virtual T VisitLetExpr(Expr.Let node)
    {
        VisitNodeOrNull(node.AnnotatedType);
        VisitNode(node.Value);
        VisitNode(node.Expression);
        return Default;
    }

    public virtual T VisitTupleExpr(Expr.Tuple node)
    {
        VisitMany(node.Values).Enumerate();
        return Default;
    }

    public virtual T VisitListExpr(Expr.List node)
    {
        VisitMany(node.Values).Enumerate();
        return Default;
    }

    public virtual T VisitAnnotatedExpr(Expr.Annotated node)
    {
        VisitNode(node.Expression);
        VisitNode(node.Annotation);
        return Default;
    }

    public virtual T VisitType(AstType node) => node switch
    {
        AstType.Unit x => VisitUnitType(x),
        AstType.Int x => VisitIntType(x),
        AstType.Bool x => VisitBoolType(x),
        AstType.Func x => VisitFuncType(x),
        AstType.Tuple x => VisitTupleType(x),
        AstType.List x => VisitListType(x),
        AstType.Var x => VisitVarType(x),
        _ => throw new UnreachableException(),
    };

    public virtual T VisitUnitType(AstType.Unit node) => Default;

    public virtual T VisitIntType(AstType.Int node) => Default;

    public virtual T VisitBoolType(AstType.Bool node) => Default;

    public virtual T VisitFuncType(AstType.Func node)
    {
        VisitNode(node.Parameter);
        VisitNode(node.Return);
        return Default;
    }

    public virtual T VisitTupleType(AstType.Tuple node)
    {
        VisitMany(node.Types).Enumerate();
        return Default;
    }

    public virtual T VisitListType(AstType.List node)
    {
        VisitNode(node.Containing);
        return Default;
    }

    public virtual T VisitVarType(AstType.Var node) => Default;
}
