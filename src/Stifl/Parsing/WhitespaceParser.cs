using System.Diagnostics.CodeAnalysis;
using Pidgin;

namespace Stifl.Parsing;

internal sealed class WhitespaceParser : Parser<char, Unit>
{
    public override bool TryParse(ref ParseState<char> state, ref PooledList<Expected<char>> expecteds, [MaybeNullWhen(false)] out Unit result)
    {
        while (state.HasCurrent)
        {
            if (char.IsWhiteSpace(state.Current))
            {
                state.Advance();
                continue;
            }

            var next = state.LookAhead(2);
            if (next.Equals("//", StringComparison.InvariantCulture))
            {
                SkipLineComment(ref state);
                continue;
            }

            break;
        }

        result = Unit.Value;
        return true;
    }

    private static void SkipLineComment(ref ParseState<char> state)
    {
        while (state.HasCurrent && state.Current != '\n') state.Advance();
    }
}
