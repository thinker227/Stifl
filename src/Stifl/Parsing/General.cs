using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace Stifl.Parsing;

/// <summary>
/// General methods for parsing.
/// </summary>
public static class General
{
    /// <summary>
    /// Parses whitespace after another parser.
    /// </summary>
    public static Parser<char, T> Whitespace<T>(this Parser<char, T> parser) =>
        parser.Before(Whitespaces);

    /// <summary>
    /// Parses a character with optional whitespace after it.
    /// </summary>
    /// <param name="character">The character to parse.</param>
    public static Parser<char, char> CharW(char character) =>
        Char(character).Whitespace();

    /// <summary>
    /// Parses a string with optional whitespace after it.
    /// </summary>
    /// <param name="str">The string to parse.</param>
    public static Parser<char, string> StringW(string str) =>
        Try(String(str).Whitespace());

    /// <summary>
    /// Parses a separated list of values.
    /// </summary>
    /// <param name="parse">The parser for the values.</param>
    /// <param name="separator">The parser for the separators.</param>
    public static Parser<char, ImmutableArray<T>> Separated<T, TSep>(
        Parser<char, T> parse,
        Parser<char, TSep> separator)
    {
        var items =
            from first in parse
            from next in separator.Then(parse).Many()
            select ImmutableArray.Create(first).AddRange(next);

        var none = FromResult(ImmutableArray<T>.Empty);

        return items.Or(none);
    }

    /// <summary>
    /// Encloses a parser by two other parsers.
    /// </summary>
    /// <param name="parser">The parser to enclose.</param>
    /// <param name="start">The starting parser.</param>
    /// <param name="end">The ending parser.</param>
    public static Parser<char, T> Enclosed<T, TStart, TEnd>(
        this Parser<char, T> parser,
        Parser<char, TStart> start,
        Parser<char, TEnd> end) =>
        start.Then(parser).Before(end);

    /// <summary>
    /// Parses an identifier.
    /// </summary>
    public static Parser<char, string> Identifier { get; } =
        Try(Letter.AtLeastOnceString().Where(s => s is not
              ("fn"
            or "let"
            or "in"
            or "if"
            or "then"
            or "else"
            or "true"
            or "false")));

    /// <summary>
    /// Parses a type annotation.
    /// </summary>
    public static Parser<char, AstType> Annotation { get; } =
        CharW(':').Then(Types.Type);
}
