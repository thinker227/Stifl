using Pidgin;
using static Pidgin.Parser<char>;
using Stifl.Parsing;

namespace Stifl;

/// <summary>
/// Methods for parsing.
/// </summary>
public static class Parse
{
    /// <summary>
    /// Parses a full input stream with leading whitespace and a trailing end of input.
    /// </summary>
    /// <param name="parse">The inner parser to run.</param>
    public static Parser<char, T> Full<T>(this Parser<char, T> parse) =>
        General.Whitespace.Then(parse).Before(End);

    /// <inheritdoc cref="General.Identifier"/>
    public static Parser<char, string> Identifier => General.Identifier;

    /// <inheritdoc cref="Declarations.Unit"/>
    public static Parser<char, Ast.Unit> Unit => Declarations.Unit;

    /// <inheritdoc cref="Declarations.Decl"/>
    public static Parser<char, Ast.Decl> Decl => Declarations.Decl;

    /// <inheritdoc cref="Declarations.Binding"/>
    public static Parser<char, Ast.Decl.Binding> Binding => Declarations.Binding;

    /// <inheritdoc cref="Expressions.Expr"/>
    public static Parser<char, Ast.Expr> Expr => Expressions.Expr;
    
    /// <inheritdoc cref="Types.Type"/>
    public static Parser<char, AstType> Type => Parsing.Types.Type;
}
