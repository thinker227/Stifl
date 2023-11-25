using Stifl.Types;

namespace Stifl.Interpret;

/// <summary>
/// A integer value.
/// </summary>
/// <param name="eval">A function to evaluate the value.</param>
public sealed class IntValue(Func<int> eval)
    : Value<int, WellKnownType>(WellKnownType.Int, eval)
{
    /// <summary>
    /// Creates a <see cref="IntValue"/> with a constant pre-evaluated value.
    /// </summary>
    /// <param name="value">The constant pre-evaluated value.</param>
    public static IntValue Const(int value)
    {
        var x = new IntValue(() => value);
        x.Eval();
        return x;
    }
}
