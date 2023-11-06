using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;
using static Stifl.Parsing.General;
using static Stifl.Ast;
using Unit = Stifl.Ast.Unit;

namespace Stifl.Parsing;

/// <summary>
/// Methods for parsing declarations.
/// </summary>
public static class Declarations
{
    /// <summary>
    /// Parses a <see cref="Decl.Binding"/>.
    /// </summary>
    public static Parser<char, Decl.Binding> Binding { get; } =
        from name in Identifier.Whitespace()
        from annotation in Annotation.Optional()
            .Before(CharW('='))
        from expr in Expressions.Expr
            .Before(CharW(';'))
        select new Decl.Binding(name, annotation.Nullable(), expr);

    /// <summary>
    /// Parses a <see cref="Ast.Decl"/>.
    /// </summary>
    public static Parser<char, Decl> Decl { get; } = Parser.OneOf(
        Binding.Cast<Decl>());

    /// <summary>
    /// Parses a <see cref="Ast.Unit"/>.
    /// </summary>
    public static Parser<char, Unit> Unit { get; } =
        Decl
            .Many()
            .Select(decls => new Unit(decls.ToImmutableArray()));
}
