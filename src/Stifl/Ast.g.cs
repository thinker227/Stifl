using System.Diagnostics.CodeAnalysis;

namespace Stifl;
#nullable enable
#pragma warning disable CS0108
/// <summary>
/// A node in an abstract syntax tree.
/// </summary>
public abstract partial record Ast
{
    /// <summary>
    /// A single unit of syntax, equivalent to a file.
    /// </summary>
    /// <param name = "Decls">The declarations in the unit.</param>
    public sealed partial record Unit(ImmutableArray<Decl> Decls) : Ast;
    /// <summary>
    /// A declaration.
    /// </summary>
    public abstract partial record Decl : Ast
    {
        /// <summary>
        /// A binding declaration.
        /// </summary>
        /// <param name = "Name">The name of the binding.</param>
        /// <param name = "AnnotatedType">The annotated type of the binding.</param>
        /// <param name = "Expression">The body expression of the binding.</param>
        public sealed partial record Binding(string Name, Type? AnnotatedType, Expr Expression) : Decl;
    }

    /// <summary>
    /// An expression.
    /// </summary>
    public abstract partial record Expr : Ast
    {
        /// <summary>
        /// A unit literal.
        /// </summary>
        public sealed partial record Unit : Expr;
        /// <summary>
        /// An undefined literal.
        /// </summary>
        public sealed partial record UndefinedLiteral : Expr;
        /// <summary>
        /// A boolean literal.
        /// </summary>
        /// <param name = "Value">The value of the literal.</param>
        public sealed partial record BoolLiteral(bool Value) : Expr;
        /// <summary>
        /// A 32-bit integer literal.
        /// </summary>
        /// <param name = "Value">The value of the literal.</param>
        public sealed partial record IntLiteral(int Value) : Expr;
        /// <summary>
        /// An identifier referencing a symbol.
        /// </summary>
        /// <param name = "Name">The name of the symbol the identifier is referencing.</param>
        public sealed partial record Identifier(string Name) : Expr;
        /// <summary>
        /// A function expression.
        /// </summary>
        /// <param name = "Parameter">The name of the function's parameter symbol.</param>
        /// <param name = "AnnotatedType">The annotated type of the function.</param>
        /// <param name = "Body">The body expression of the function.</param>
        public sealed partial record Func(string Parameter, Type? AnnotatedType, Expr Body) : Expr;
        /// <summary>
        /// An if-else-then expression.
        /// </summary>
        /// <param name = "Condition">The if condition.</param>
        /// <param name = "IfTrue">The expression returned if the condition is true.</param>
        /// <param name = "IfFalse">The expression returned if the condition if false.</param>
        public sealed partial record If(Expr Condition, Expr IfTrue, Expr IfFalse) : Expr;
        /// <summary>
        /// A call expression.
        /// </summary>
        /// <param name = "Function">The expression evaluating to the function to call.</param>
        /// <param name = "Argument">The argument to call the function with.</param>
        public sealed partial record Call(Expr Function, Expr Argument) : Expr;
        /// <summary>
        /// A let-in expression.
        /// </summary>
        /// <param name = "Name">The name of the declared variable.</param>
        /// <param name = "AnnotatedType">The annotated type of the variable.</param>
        /// <param name = "Value">The value of the variable.</param>
        /// <param name = "Expression">The expression to evaluate with the variable in scope.</param>
        public sealed partial record Let(string Name, Type? AnnotatedType, Expr Value, Expr Expression) : Expr;
        /// <summary>
        /// A tuple expression.
        /// </summary>
        /// <param name = "Values">The values in the tuple.</param>
        public sealed partial record Tuple(ImmutableArray<Expr> Values) : Expr;
        /// <summary>
        /// A list expression.
        /// </summary>
        /// <param name = "Values">The values in the list.</param>
        public sealed partial record List(ImmutableArray<Expr> Values) : Expr;
        /// <summary>
        /// A type annotation expression.
        /// </summary>
        /// <param name = "Expression">The annotated expression.</param>
        /// <param name = "Annotation">The type the expression is annotated with</param>
        public sealed partial record Annotated(Expr Expression, Type Annotation) : Expr;
    }

    /// <summary>
    /// A type.
    /// </summary>
    public abstract partial record Type : Ast
    {
        /// <summary>
        /// The unit type.
        /// </summary>
        public sealed partial record Unit : Type;
        /// <summary>
        /// The 32-bit integer type.
        /// </summary>
        public sealed partial record Int : Type;
        /// <summary>
        /// The boolean type.
        /// </summary>
        public sealed partial record Bool : Type;
        /// <summary>
        /// A function type.
        /// </summary>
        /// <param name = "Parameter">The type of the parameter to the function.</param>
        /// <param name = "Return">The return type of the function.</param>
        public sealed partial record Func(Type Parameter, Type Return) : Type;
        /// <summary>
        /// A tuple type.
        /// </summary>
        /// <param name = "Types">The types of the values in the tuple.</param>
        public sealed partial record Tuple(ImmutableArray<Type> Types) : Type;
        /// <summary>
        /// A list type.
        /// </summary>
        /// <param name = "Containing">The type of the elements in the list.</param>
        public sealed partial record List(Type Containing) : Type;
        /// <summary>
        /// A type parameter.
        /// </summary>
        /// <param name = "Name">The name of the type parameter.</param>
        public sealed partial record Var(string Name) : Type;
    }
}

partial record Ast : INode<Ast>
{
    private static IEnumerable<T> EmptyIfNull<T>(T? value) => value is not null ? [value] : [];
    public IEnumerable<Ast> Children() => this switch
    {
        Ast.Unit x => [..x.Decls],
        Ast.Decl.Binding x => [..EmptyIfNull(x.AnnotatedType), x.Expression],
        Ast.Expr.Unit x => [],
        Ast.Expr.UndefinedLiteral x => [],
        Ast.Expr.BoolLiteral x => [],
        Ast.Expr.IntLiteral x => [],
        Ast.Expr.Identifier x => [],
        Ast.Expr.Func x => [..EmptyIfNull(x.AnnotatedType), x.Body],
        Ast.Expr.If x => [x.Condition, x.IfTrue, x.IfFalse],
        Ast.Expr.Call x => [x.Function, x.Argument],
        Ast.Expr.Let x => [..EmptyIfNull(x.AnnotatedType), x.Value, x.Expression],
        Ast.Expr.Tuple x => [..x.Values],
        Ast.Expr.List x => [..x.Values],
        Ast.Expr.Annotated x => [x.Expression, x.Annotation],
        Ast.Type.Unit x => [],
        Ast.Type.Int x => [],
        Ast.Type.Bool x => [],
        Ast.Type.Func x => [x.Parameter, x.Return],
        Ast.Type.Tuple x => [..x.Types],
        Ast.Type.List x => [x.Containing],
        Ast.Type.Var x => [],
        _ => throw new UnreachableException()};
}

/// <summary>
/// Visits AST nodes.
/// </summary>
/// <typeparam name = "T">The type which the visitor returns.</typeparam>
public abstract class AstVisitor<T>
    where T : notnull
{
    /// <summary>
    /// The default value of a visit.
    /// </summary>
    protected abstract T Default { get; }

    /// <summary>
    /// Called before a node is visited.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    protected virtual void BeforeVisit(Ast node)
    {
    }

    /// <summary>
    /// Called after a node has been visited.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    protected virtual void AfterVisit(Ast node)
    {
    }

    /// <summary>
    /// Filters nodes which should be visited.
    /// </summary>
    /// <param name = "node">The node to filter.</param>
    /// <returns>Whether the node and its children should be visited.</returns>
    protected virtual bool Filter(Ast node) => true;
    /// <summary>
    /// Visits many nodes and returns a sequence of return values.
    /// </summary>
    /// <typeparam name = "TNode">The type of the nodes to visit.</typeparam>
    /// <param name = "nodes">The nodes to visit.</param>
    public IReadOnlyList<T> VisitMany<TNode>(IEnumerable<TNode> nodes)
        where TNode : Ast => nodes.Select(VisitNode).ToList()!;
    /// <summary>
    /// Visits a node.
    /// </summary>
    /// <param name = "node">The node to visit.</param>
    /// <returns>The return value of the visit.</returns>
    [return: NotNullIfNotNull(nameof(node))]
    public T? VisitNode(Ast? node)
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

    /// <summary>
    /// Visits a node of type <see cref = "Ast"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
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

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Unit"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitUnit(Ast.Unit node)
    {
        VisitMany(node.Decls);
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Decl"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
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

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Decl.Binding"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitBindingDecl(Ast.Decl.Binding node)
    {
        VisitNode(node.AnnotatedType);
        VisitNode(node.Expression);
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
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

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.Unit"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitUnitExpr(Ast.Expr.Unit node)
    {
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.UndefinedLiteral"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitUndefinedLiteralExpr(Ast.Expr.UndefinedLiteral node)
    {
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.BoolLiteral"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitBoolLiteralExpr(Ast.Expr.BoolLiteral node)
    {
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.IntLiteral"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitIntLiteralExpr(Ast.Expr.IntLiteral node)
    {
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.Identifier"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitIdentifierExpr(Ast.Expr.Identifier node)
    {
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.Func"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitFuncExpr(Ast.Expr.Func node)
    {
        VisitNode(node.AnnotatedType);
        VisitNode(node.Body);
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.If"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitIfExpr(Ast.Expr.If node)
    {
        VisitNode(node.Condition);
        VisitNode(node.IfTrue);
        VisitNode(node.IfFalse);
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.Call"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitCallExpr(Ast.Expr.Call node)
    {
        VisitNode(node.Function);
        VisitNode(node.Argument);
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.Let"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitLetExpr(Ast.Expr.Let node)
    {
        VisitNode(node.AnnotatedType);
        VisitNode(node.Value);
        VisitNode(node.Expression);
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.Tuple"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitTupleExpr(Ast.Expr.Tuple node)
    {
        VisitMany(node.Values);
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.List"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitListExpr(Ast.Expr.List node)
    {
        VisitMany(node.Values);
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.Annotated"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitAnnotatedExpr(Ast.Expr.Annotated node)
    {
        VisitNode(node.Expression);
        VisitNode(node.Annotation);
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
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

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type.Unit"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitUnitType(Ast.Type.Unit node)
    {
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type.Int"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitIntType(Ast.Type.Int node)
    {
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type.Bool"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitBoolType(Ast.Type.Bool node)
    {
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type.Func"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitFuncType(Ast.Type.Func node)
    {
        VisitNode(node.Parameter);
        VisitNode(node.Return);
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type.Tuple"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitTupleType(Ast.Type.Tuple node)
    {
        VisitMany(node.Types);
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type.List"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitListType(Ast.Type.List node)
    {
        VisitNode(node.Containing);
        return Default;
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type.Var"/>.
    /// </summary>
    /// <param name = "node">The node which is being visited.</param>
    /// <returns>The return value of the visit.</returns>
    public virtual T VisitVarType(Ast.Type.Var node)
    {
        return Default;
    }
}

/// <summary>
/// Visits AST nodes.
/// </summary>
public abstract class AstVisitor
{
    /// <summary>
    /// Called before a node is visited.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    protected virtual void BeforeVisit(Ast node)
    {
    }

    /// <summary>
    /// Called after a node has been visited.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    protected virtual void AfterVisit(Ast node)
    {
    }

    /// <summary>
    /// Filters nodes which should be visited.
    /// </summary>
    /// <param name = "node">The node to filter.</param>
    /// <returns>Whether the node and its children should be visited.</returns>
    protected virtual bool Filter(Ast node) => true;
    /// <summary>
    /// Visits many nodes and returns a sequence of return values.
    /// </summary>
    /// <typeparam name = "TNode">The type of the nodes to visit.</typeparam>
    /// <param name = "nodes">The nodes to visit.</param>
    public void VisitMany<TNode>(IEnumerable<TNode> nodes)
        where TNode : Ast
    {
        foreach (var x in nodes)
            VisitNode(x);
    }

    /// <summary>
    /// Visits a node.
    /// </summary>
    /// <param name = "node">The node to visit.</param>
    public void VisitNode(Ast? node)
    {
        if (node is null)
            return;
        if (!Filter(node))
            return;
        BeforeVisit(node);
        VisitAst(node);
        AfterVisit(node);
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
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

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Unit"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitUnit(Ast.Unit node)
    {
        VisitMany(node.Decls);
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Decl"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
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

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Decl.Binding"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitBindingDecl(Ast.Decl.Binding node)
    {
        VisitNode(node.AnnotatedType);
        VisitNode(node.Expression);
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
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

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.Unit"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitUnitExpr(Ast.Expr.Unit node)
    {
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.UndefinedLiteral"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitUndefinedLiteralExpr(Ast.Expr.UndefinedLiteral node)
    {
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.BoolLiteral"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitBoolLiteralExpr(Ast.Expr.BoolLiteral node)
    {
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.IntLiteral"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitIntLiteralExpr(Ast.Expr.IntLiteral node)
    {
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.Identifier"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitIdentifierExpr(Ast.Expr.Identifier node)
    {
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.Func"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitFuncExpr(Ast.Expr.Func node)
    {
        VisitNode(node.AnnotatedType);
        VisitNode(node.Body);
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.If"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitIfExpr(Ast.Expr.If node)
    {
        VisitNode(node.Condition);
        VisitNode(node.IfTrue);
        VisitNode(node.IfFalse);
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.Call"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitCallExpr(Ast.Expr.Call node)
    {
        VisitNode(node.Function);
        VisitNode(node.Argument);
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.Let"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitLetExpr(Ast.Expr.Let node)
    {
        VisitNode(node.AnnotatedType);
        VisitNode(node.Value);
        VisitNode(node.Expression);
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.Tuple"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitTupleExpr(Ast.Expr.Tuple node)
    {
        VisitMany(node.Values);
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.List"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitListExpr(Ast.Expr.List node)
    {
        VisitMany(node.Values);
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Expr.Annotated"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitAnnotatedExpr(Ast.Expr.Annotated node)
    {
        VisitNode(node.Expression);
        VisitNode(node.Annotation);
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
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

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type.Unit"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitUnitType(Ast.Type.Unit node)
    {
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type.Int"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitIntType(Ast.Type.Int node)
    {
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type.Bool"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitBoolType(Ast.Type.Bool node)
    {
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type.Func"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitFuncType(Ast.Type.Func node)
    {
        VisitNode(node.Parameter);
        VisitNode(node.Return);
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type.Tuple"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitTupleType(Ast.Type.Tuple node)
    {
        VisitMany(node.Types);
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type.List"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitListType(Ast.Type.List node)
    {
        VisitNode(node.Containing);
    }

    /// <summary>
    /// Visits a node of type <see cref = "Ast.Type.Var"/>.
    /// </summary>
    /// <param name = "node">The node being visited.</param>
    public virtual void VisitVarType(Ast.Type.Var node)
    {
    }
}