using Stifl.Types;

namespace Stifl.Interpret;

/// <summary>
/// A boolean value.
/// </summary>
/// <param name="eval">A function to evaluate the value.</param>
public sealed class BoolValue(Func<bool> eval)
    : Value<bool, WellKnownType>(WellKnownType.Bool, eval)
{
    /// <summary>
    /// Creates a <see cref="BoolValue"/> with a constant pre-evaluated value.
    /// </summary>
    /// <param name="value">The constant pre-evaluated value.</param>
    public static BoolValue Const(bool value)
    {
        var x = new BoolValue(() => value);
        x.Eval();
        return x;
    }
}
