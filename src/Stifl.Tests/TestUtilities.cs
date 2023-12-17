using Pidgin;
using Stifl.Parsing;

namespace Stifl.Tests;

public static class TestUtilities
{
    /// <summary>
    /// Parses a string into an AST node.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <typeparam name="T">The node type to parse the string into.</typeparam>
    /// <returns>The parsed AST node.</returns>
    public static T Parse<T>(this string s) where T : IAstParsable<T> =>
        T.Parser.Full().ParseOrThrow(s);
}
