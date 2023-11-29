using System.Diagnostics.CodeAnalysis;

namespace Stifl;
#nullable enable
#pragma warning disable CS0108
public abstract partial record Ast
{
    public sealed partial record Unit(ImmutableArray<Decl> Decls) : Ast;
    public abstract partial record Decl : Ast
    {
        public sealed partial record Binding(string Name, Type? AnnotatedType, Expr Expression) : Decl;
    }

    public abstract partial record Expr : Ast
    {
        public sealed partial record Unit : Expr;
        public sealed partial record UndefinedLiteral : Expr;
        public sealed partial record BoolLiteral(bool Value) : Expr;
        public sealed partial record IntLiteral(int Value) : Expr;
        public sealed partial record Identifier(string Name) : Expr;
        public sealed partial record Func(string Parameter, Type? AnnotatedType, Expr Body) : Expr;
        public sealed partial record If(Expr Condition, Expr IfTrue, Expr IfFalse) : Expr;
        public sealed partial record Call(Expr Function, Expr Argument) : Expr;
        public sealed partial record Let(string Name, Type? AnnotatedType, Expr Value, Expr Expression) : Expr;
        public sealed partial record Tuple(ImmutableArray<Expr> Values) : Expr;
        public sealed partial record List(ImmutableArray<Expr> Values) : Expr;
        public sealed partial record Annotated(Expr Expression, Type Annotation) : Expr;
    }

    public abstract partial record Type : Ast
    {
        public sealed partial record Unit : Type;
        public sealed partial record Int : Type;
        public sealed partial record Bool : Type;
        public sealed partial record Func(Type Parameter, Type Return) : Type;
        public sealed partial record Tuple(ImmutableArray<Type> Types) : Type;
        public sealed partial record List(Type Containing) : Type;
        public sealed partial record Var(string Name) : Type;
    }
}

public abstract class AstVisitor<T>
    where T : notnull
{
    protected abstract T Default { get; }

    protected virtual void BeforeVisit(Ast node)
    {
    }

    protected virtual void AfterVisit(Ast node)
    {
    }

    protected virtual bool Filter(Ast node) => true;
    public IReadOnlyList<T> VisitMany<TNode>(IEnumerable<TNode> nodes)
        where TNode : Ast => nodes.Select(VisitNode).ToList()!;
    [return: NotNullIfNotNull(nameof(node))]
    public virtual T? VisitNode(Ast? node)
    {
        if (node is null)
            return default;
        if (!Filter(node))
            return Default;
        BeforeVisit(node);
        var x = VisitAst(node);
        AfterVisit(node);
        return x;
    }

    public virtual T VisitAst(Ast node)
    {
        switch (node)
        {
            case Ast.Unit x:
                return VisitUnit(x);
            case Ast.Decl x:
                return VisitDecl(x);
            case Ast.Expr x:
                return VisitExpr(x);
            case Ast.Type x:
                return VisitType(x);
            default:
                throw new UnreachableException();
        }
    }

    public virtual T VisitUnit(Ast.Unit node)
    {
        VisitMany(node.Decls);
        return Default;
    }

    public virtual T VisitDecl(Ast.Decl node)
    {
        switch (node)
        {
            case Ast.Decl.Binding x:
                return VisitBindingDecl(x);
            default:
                throw new UnreachableException();
        }
    }

    public virtual T VisitBindingDecl(Ast.Decl.Binding node)
    {
        VisitNode(node.AnnotatedType);
        VisitNode(node.Expression);
        return Default;
    }

    public virtual T VisitExpr(Ast.Expr node)
    {
        switch (node)
        {
            case Ast.Expr.Unit x:
                return VisitUnitExpr(x);
            case Ast.Expr.UndefinedLiteral x:
                return VisitUndefinedLiteralExpr(x);
            case Ast.Expr.BoolLiteral x:
                return VisitBoolLiteralExpr(x);
            case Ast.Expr.IntLiteral x:
                return VisitIntLiteralExpr(x);
            case Ast.Expr.Identifier x:
                return VisitIdentifierExpr(x);
            case Ast.Expr.Func x:
                return VisitFuncExpr(x);
            case Ast.Expr.If x:
                return VisitIfExpr(x);
            case Ast.Expr.Call x:
                return VisitCallExpr(x);
            case Ast.Expr.Let x:
                return VisitLetExpr(x);
            case Ast.Expr.Tuple x:
                return VisitTupleExpr(x);
            case Ast.Expr.List x:
                return VisitListExpr(x);
            case Ast.Expr.Annotated x:
                return VisitAnnotatedExpr(x);
            default:
                throw new UnreachableException();
        }
    }

    public virtual T VisitUnitExpr(Ast.Expr.Unit node)
    {
        return Default;
    }

    public virtual T VisitUndefinedLiteralExpr(Ast.Expr.UndefinedLiteral node)
    {
        return Default;
    }

    public virtual T VisitBoolLiteralExpr(Ast.Expr.BoolLiteral node)
    {
        return Default;
    }

    public virtual T VisitIntLiteralExpr(Ast.Expr.IntLiteral node)
    {
        return Default;
    }

    public virtual T VisitIdentifierExpr(Ast.Expr.Identifier node)
    {
        return Default;
    }

    public virtual T VisitFuncExpr(Ast.Expr.Func node)
    {
        VisitNode(node.AnnotatedType);
        VisitNode(node.Body);
        return Default;
    }

    public virtual T VisitIfExpr(Ast.Expr.If node)
    {
        VisitNode(node.Condition);
        VisitNode(node.IfTrue);
        VisitNode(node.IfFalse);
        return Default;
    }

    public virtual T VisitCallExpr(Ast.Expr.Call node)
    {
        VisitNode(node.Function);
        VisitNode(node.Argument);
        return Default;
    }

    public virtual T VisitLetExpr(Ast.Expr.Let node)
    {
        VisitNode(node.AnnotatedType);
        VisitNode(node.Value);
        VisitNode(node.Expression);
        return Default;
    }

    public virtual T VisitTupleExpr(Ast.Expr.Tuple node)
    {
        VisitMany(node.Values);
        return Default;
    }

    public virtual T VisitListExpr(Ast.Expr.List node)
    {
        VisitMany(node.Values);
        return Default;
    }

    public virtual T VisitAnnotatedExpr(Ast.Expr.Annotated node)
    {
        VisitNode(node.Expression);
        VisitNode(node.Annotation);
        return Default;
    }

    public virtual T VisitType(Ast.Type node)
    {
        switch (node)
        {
            case Ast.Type.Unit x:
                return VisitUnitType(x);
            case Ast.Type.Int x:
                return VisitIntType(x);
            case Ast.Type.Bool x:
                return VisitBoolType(x);
            case Ast.Type.Func x:
                return VisitFuncType(x);
            case Ast.Type.Tuple x:
                return VisitTupleType(x);
            case Ast.Type.List x:
                return VisitListType(x);
            case Ast.Type.Var x:
                return VisitVarType(x);
            default:
                throw new UnreachableException();
        }
    }

    public virtual T VisitUnitType(Ast.Type.Unit node)
    {
        return Default;
    }

    public virtual T VisitIntType(Ast.Type.Int node)
    {
        return Default;
    }

    public virtual T VisitBoolType(Ast.Type.Bool node)
    {
        return Default;
    }

    public virtual T VisitFuncType(Ast.Type.Func node)
    {
        VisitNode(node.Parameter);
        VisitNode(node.Return);
        return Default;
    }

    public virtual T VisitTupleType(Ast.Type.Tuple node)
    {
        VisitMany(node.Types);
        return Default;
    }

    public virtual T VisitListType(Ast.Type.List node)
    {
        VisitNode(node.Containing);
        return Default;
    }

    public virtual T VisitVarType(Ast.Type.Var node)
    {
        return Default;
    }
}

public abstract class AstVisitor
{
    protected virtual void BeforeVisit(Ast node)
    {
    }

    protected virtual void AfterVisit(Ast node)
    {
    }

    protected virtual bool Filter(Ast node) => true;
    public void VisitMany<TNode>(IEnumerable<TNode> nodes)
        where TNode : Ast
    {
        foreach (var x in nodes)
            VisitNode(x);
    }

    public virtual void VisitNode(Ast? node)
    {
        if (node is null)
            return;
        if (!Filter(node))
            return;
        BeforeVisit(node);
        VisitAst(node);
        AfterVisit(node);
    }

    public virtual void VisitAst(Ast node)
    {
        switch (node)
        {
            case Ast.Unit x:
                VisitUnit(x);
                break;
            case Ast.Decl x:
                VisitDecl(x);
                break;
            case Ast.Expr x:
                VisitExpr(x);
                break;
            case Ast.Type x:
                VisitType(x);
                break;
            default:
                throw new UnreachableException();
        }
    }

    public virtual void VisitUnit(Ast.Unit node)
    {
        VisitMany(node.Decls);
    }

    public virtual void VisitDecl(Ast.Decl node)
    {
        switch (node)
        {
            case Ast.Decl.Binding x:
                VisitBindingDecl(x);
                break;
            default:
                throw new UnreachableException();
        }
    }

    public virtual void VisitBindingDecl(Ast.Decl.Binding node)
    {
        VisitNode(node.AnnotatedType);
        VisitNode(node.Expression);
    }

    public virtual void VisitExpr(Ast.Expr node)
    {
        switch (node)
        {
            case Ast.Expr.Unit x:
                VisitUnitExpr(x);
                break;
            case Ast.Expr.UndefinedLiteral x:
                VisitUndefinedLiteralExpr(x);
                break;
            case Ast.Expr.BoolLiteral x:
                VisitBoolLiteralExpr(x);
                break;
            case Ast.Expr.IntLiteral x:
                VisitIntLiteralExpr(x);
                break;
            case Ast.Expr.Identifier x:
                VisitIdentifierExpr(x);
                break;
            case Ast.Expr.Func x:
                VisitFuncExpr(x);
                break;
            case Ast.Expr.If x:
                VisitIfExpr(x);
                break;
            case Ast.Expr.Call x:
                VisitCallExpr(x);
                break;
            case Ast.Expr.Let x:
                VisitLetExpr(x);
                break;
            case Ast.Expr.Tuple x:
                VisitTupleExpr(x);
                break;
            case Ast.Expr.List x:
                VisitListExpr(x);
                break;
            case Ast.Expr.Annotated x:
                VisitAnnotatedExpr(x);
                break;
            default:
                throw new UnreachableException();
        }
    }

    public virtual void VisitUnitExpr(Ast.Expr.Unit node)
    {
    }

    public virtual void VisitUndefinedLiteralExpr(Ast.Expr.UndefinedLiteral node)
    {
    }

    public virtual void VisitBoolLiteralExpr(Ast.Expr.BoolLiteral node)
    {
    }

    public virtual void VisitIntLiteralExpr(Ast.Expr.IntLiteral node)
    {
    }

    public virtual void VisitIdentifierExpr(Ast.Expr.Identifier node)
    {
    }

    public virtual void VisitFuncExpr(Ast.Expr.Func node)
    {
        VisitNode(node.AnnotatedType);
        VisitNode(node.Body);
    }

    public virtual void VisitIfExpr(Ast.Expr.If node)
    {
        VisitNode(node.Condition);
        VisitNode(node.IfTrue);
        VisitNode(node.IfFalse);
    }

    public virtual void VisitCallExpr(Ast.Expr.Call node)
    {
        VisitNode(node.Function);
        VisitNode(node.Argument);
    }

    public virtual void VisitLetExpr(Ast.Expr.Let node)
    {
        VisitNode(node.AnnotatedType);
        VisitNode(node.Value);
        VisitNode(node.Expression);
    }

    public virtual void VisitTupleExpr(Ast.Expr.Tuple node)
    {
        VisitMany(node.Values);
    }

    public virtual void VisitListExpr(Ast.Expr.List node)
    {
        VisitMany(node.Values);
    }

    public virtual void VisitAnnotatedExpr(Ast.Expr.Annotated node)
    {
        VisitNode(node.Expression);
        VisitNode(node.Annotation);
    }

    public virtual void VisitType(Ast.Type node)
    {
        switch (node)
        {
            case Ast.Type.Unit x:
                VisitUnitType(x);
                break;
            case Ast.Type.Int x:
                VisitIntType(x);
                break;
            case Ast.Type.Bool x:
                VisitBoolType(x);
                break;
            case Ast.Type.Func x:
                VisitFuncType(x);
                break;
            case Ast.Type.Tuple x:
                VisitTupleType(x);
                break;
            case Ast.Type.List x:
                VisitListType(x);
                break;
            case Ast.Type.Var x:
                VisitVarType(x);
                break;
            default:
                throw new UnreachableException();
        }
    }

    public virtual void VisitUnitType(Ast.Type.Unit node)
    {
    }

    public virtual void VisitIntType(Ast.Type.Int node)
    {
    }

    public virtual void VisitBoolType(Ast.Type.Bool node)
    {
    }

    public virtual void VisitFuncType(Ast.Type.Func node)
    {
        VisitNode(node.Parameter);
        VisitNode(node.Return);
    }

    public virtual void VisitTupleType(Ast.Type.Tuple node)
    {
        VisitMany(node.Types);
    }

    public virtual void VisitListType(Ast.Type.List node)
    {
        VisitNode(node.Containing);
    }

    public virtual void VisitVarType(Ast.Type.Var node)
    {
    }
}