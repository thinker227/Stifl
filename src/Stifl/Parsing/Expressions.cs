using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;
using static Stifl.Parsing.General;
using static Stifl.Ast;

namespace Stifl.Parsing;

/// <summary>
/// Methods for parsing expressions.
/// </summary>
public static class Expressions
{
    /// <summary>
    /// Parses a <see cref="Expr.List"/>.
    /// </summary>
    private static Parser<char, Expr> ListExpr =>
        Separated(RecExpr!, CharW(','))
            .Map<Expr>(xs => new Expr.List(xs))
            .Enclosed(CharW('['), CharW(']'));

    /// <summary>
    /// Parses a unit, parenthesized, or tuple type.
    /// </summary>
    private static Parser<char, Expr> UnitOrParensOrTupleExpr =>
        Separated(RecExpr!, CharW(',')).Map(xs => xs switch
        {
            [] => new Expr.Unit(),
            [var x] => x,
            _ => new Expr.Tuple(xs)
        })
        .Enclosed(CharW('('), CharW(')'));

    /// <summary>
    /// Parses a <see cref="Expr.BoolLiteral"/>.
    /// </summary>
    private static Parser<char, Expr> BoolLiteralExpr { get; } =
        StringW("true").Select<Expr>(_ => new Expr.BoolLiteral(true))
            .Or(StringW("false").Select<Expr>(_ => new Expr.BoolLiteral(false)));

    /// <summary>
    /// Parses a <see cref="Expr.Func"/>.
    /// </summary>
    private static Parser<char, Expr> FuncExpr =>
        from parameter in StringW("fn")
            .Then(Identifier.Whitespace())
        from annotation in Annotation.Optional()
        from body in StringW("=>")
            .Then(RecExpr!)
        select (Expr)new Expr.Func(parameter, annotation.Nullable(), body);

    /// <summary>
    /// Parses a <see cref="Expr.If"/>.
    /// </summary>
    private static Parser<char, Expr> IfExpr =>
        from condition in StringW("if").Then(RecExpr!)
        from ifTrue in StringW("then").Then(RecExpr!)
        from ifFalse in StringW("else").Then(RecExpr!)
        select (Expr)new Expr.If(condition, ifTrue, ifFalse);

    /// <summary>
    /// Parses a <see cref="Expr.Let"/>
    /// </summary>
    private static Parser<char, Expr> LetExpr =>
        from name in StringW("let").Then(Identifier.Whitespace())
        from annotation in Annotation.Optional()
        from value in CharW('=').Then(RecExpr!)
        from expression in StringW("in").Then(RecExpr!)
        select (Expr)new Expr.Let(name, annotation.Nullable(), value, expression);

    /// <summary>
    /// Parses expressions other than call and annotation expressions.
    /// </summary>
    private static Parser<char, Expr> ExprCore => Parser.OneOf(
        UnitOrParensOrTupleExpr,
        CharW('?').ThenReturn<Expr>(new Expr.UndefinedLiteral()),
        BoolLiteralExpr,
        Int(10).Whitespace().Select<Expr>(x => new Expr.IntLiteral(x)),
        FuncExpr,
        IfExpr,
        LetExpr,
        ListExpr,
        Identifier.Whitespace().Select<Expr>(ident => new Expr.Identifier(ident)));

    /// <summary>
    /// Recursively parses a left-associative call expression.
    /// </summary>
    private static Parser<char, Expr> CallExpr()
    {
        static Parser<char, Expr> Call(Expr acc) =>
            ExprCore.Optional().Bind(expr => expr.Match(
                x => Call(new Expr.Call(acc, x)),
                () => FromResult(acc)));

        return ExprCore.Bind(Call);
    }

    /// <summary>
    /// Parses an expression with an optional type annotation at the end.
    /// </summary>
    private static Parser<char, Expr> AnnotationExpr =>
        from expr in CallExpr()
        from annotation in Annotation.Optional()
        select annotation.Match(
            an => new Expr.Annotated(expr, an),
            () => expr);

    private static Parser<char, Expr> RecExpr => Rec(() => Expr!);

    /// <summary>
    /// Parses an <see cref="Ast.Expr"/>.
    /// </summary>
    public static Parser<char, Expr> Expr => AnnotationExpr;
}
