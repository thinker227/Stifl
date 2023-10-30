using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;
using static Stifl.Ast;
using Unit = Stifl.Ast.Unit;

namespace Stifl;

/// <summary>
/// Parses AST nodes.
/// </summary>
public static class Parse
{
    private static Parser<char, T> OneOf<T>(params Parser<char, T>[] parsers) => Parser.OneOf(parsers);

    /// <summary>
    /// Returns the value of the <see cref="Maybe{T}"/> as nullable.
    /// </summary>
    private static T? Nullable<T>(this Maybe<T> maybe) => maybe.GetValueOrDefault();

    /// <summary>
    /// Parses whitespace after another parser.
    /// </summary>
    private static Parser<char, T> Whitespace<T>(this Parser<char, T> parser) =>
        parser.Before(Whitespaces);

    /// <summary>
    /// Parses a character with optional whitespace after it.
    /// </summary>
    /// <param name="character">The character to parse.</param>
    private static Parser<char, char> CharW(char character) =>
        Char(character).Before(Whitespaces);

    /// <summary>
    /// Parses a string with optional whitespace after it.
    /// </summary>
    /// <param name="str">The string to parse.</param>
    private static Parser<char, string> StringW(string str) =>
        Try(String(str).Before(Whitespaces));

    /// <summary>
    /// Parses an identifier.
    /// </summary>
    public static Parser<char, string> Identifier =>
        Try(Letter.AtLeastOnceString().Where(s => s is not
              ("fn"
            or "let"
            or "in"
            or "if"
            or "then"
            or "else"
            or "true"
            or "false")));

    private static Parser<char, AstType> UnitOrParensType =>
        CharW('(').Then(OneOf(
            CharW(')').ThenReturn<AstType>(new AstType.Unit()),
            RecType.Before(CharW(')'))));

    /// <summary>
    /// Parses types other than function types.
    /// </summary>
    private static Parser<char, AstType> TypeCore => OneOf(
        UnitOrParensType,
        StringW("Int").ThenReturn<AstType>(new AstType.Int()),
        StringW("Bool").ThenReturn<AstType>(new AstType.Bool()),
        CharW('\'').Then(Identifier.Whitespace().Select<AstType>(ident => new AstType.Var(ident))));

    /// <summary>
    /// Recursively parses a right-associative function type.
    /// </summary>
    private static Parser<char, AstType> FuncType =>
        from t in TypeCore
        from fn in StringW("->").Then(RecType).Optional()
        select fn.Match(
            r => new AstType.Func(t, r),
            () => t);

    private static Parser<char, AstType> RecType { get; } = Rec(() => Type);

    /// <summary>
    /// Parses a <see cref="Ast.AstType"/>.
    /// </summary>
    public static Parser<char, AstType> Type => FuncType;

    /// <summary>
    /// Parses a type annotation.
    /// </summary>
    private static Parser<char, AstType> Annotation =>
        CharW(':').Then(Type);

    private static Parser<char, Expr> UnitOrParensExpr =>
        CharW('(').Then(OneOf(
            CharW(')').ThenReturn<Expr>(new Expr.Unit()),
            RecExpr.Before(CharW(')'))));

    /// <summary>
    /// Parses a <see cref="Expr.BoolLiteral"/>.
    /// </summary>
    private static Parser<char, Expr> BoolLiteralExpr =>
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
            .Then(RecExpr)
        select (Expr)new Expr.Func(parameter, annotation.Nullable(), body);

    /// <summary>
    /// Parses a <see cref="Expr.If"/>.
    /// </summary>
    private static Parser<char, Expr> IfExpr =>
        from condition in StringW("if").Then(RecExpr)
        from ifTrue in StringW("then").Then(RecExpr)
        from ifFalse in StringW("else").Then(RecExpr)
        select (Expr)new Expr.If(condition, ifTrue, ifFalse);

    /// <summary>
    /// Parses a <see cref="Expr.Let"/>
    /// </summary>
    private static Parser<char, Expr> LetExpr =>
        from name in StringW("let").Then(Identifier.Whitespace())
        from annotation in Annotation.Optional()
        from value in CharW('=').Then(RecExpr)
        from expression in StringW("in").Then(RecExpr)
        select (Expr)new Expr.Let(name, annotation.Nullable(), value, expression);

    /// <summary>
    /// Parses expressions other than call and annotation expressions.
    /// </summary>
    private static Parser<char, Expr> ExprCore => OneOf(
        UnitOrParensExpr,
        CharW('?').ThenReturn<Expr>(new Expr.UndefinedLiteral()),
        BoolLiteralExpr,
        Int(10).Whitespace().Select<Expr>(x => new Expr.IntLiteral(x)),
        FuncExpr,
        IfExpr,
        LetExpr,
        Identifier.Whitespace().Select<Expr>(ident => new Expr.Identifier(ident)));

    /// <summary>
    /// Recursively parses a left-associative call expression.
    /// </summary>
    /// <returns></returns>
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

    private static Parser<char, Expr> RecExpr { get; } = Rec(() => Expr);

    /// <summary>
    /// Parses an <see cref="Ast.Expr"/>.
    /// </summary>
    public static Parser<char, Expr> Expr => AnnotationExpr;

    /// <summary>
    /// Parses a <see cref="Decl.Binding"/>.
    /// </summary>
    public static Parser<char, Decl.Binding> Binding =>
        from name in Identifier.Whitespace()
        from annotation in Annotation.Optional()
            .Before(CharW('='))
        from expr in Expr
            .Before(CharW(';'))
        select new Decl.Binding(name, annotation.Nullable(), expr);

    /// <summary>
    /// Parses a <see cref="Ast.Decl"/>.
    /// </summary>
    public static Parser<char, Decl> Decl => OneOf(
        Binding.Cast<Decl>());

    /// <summary>
    /// Parses a <see cref="Ast.Unit"/>.
    /// </summary>
    public static Parser<char, Unit> Unit =>
        Decl
            .Many()
            .Select(decls => new Unit(decls.ToImmutableArray()));
}
