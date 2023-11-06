using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;
using static Stifl.Parsing.General;

namespace Stifl.Parsing;

/// <summary>
/// Methods for parsing types.
/// </summary>
public static class Types
{
    /// <summary>
    /// Parses a <see cref="AstType.List"/>.
    /// </summary>
    private static Parser<char, AstType> ListType =>
        RecType.Map<AstType>(t => new AstType.List(t))
            .Enclosed(CharW('['), CharW(']'));

    /// <summary>
    /// Parses a unit, parenthesized, or tuple type.
    /// </summary>
    private static Parser<char, AstType> UnitOrParensOrTupleType =>
        Separated(RecType, CharW(',')).Map(xs => xs switch
        {
            [] => new AstType.Unit(),
            [var x] => x,
            _ => new AstType.Tuple(xs)
        })
        .Enclosed(CharW('('), CharW(')'));

    /// <summary>
    /// Parses types other than function types.
    /// </summary>
    private static Parser<char, AstType> TypeCore => Parser.OneOf(
        UnitOrParensOrTupleType,
        StringW("Int").ThenReturn<AstType>(new AstType.Int()),
        StringW("Bool").ThenReturn<AstType>(new AstType.Bool()),
        ListType,
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
}
