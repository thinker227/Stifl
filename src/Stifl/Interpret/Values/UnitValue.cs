using Pidgin;
using Stifl.Types;

namespace Stifl.Interpret;

/// <summary>
/// A unit value.
/// </summary>
/// <param name="eval">A function to evaluate the value.
/// Even though the unit type only has one possible value,
/// this function might be used to perform side-effects in the interpreter.</param>
public sealed class UnitValue(Func<Unit> eval)
    : Value<Unit, WellKnownType>(WellKnownType.Unit, eval)
{
    /// <summary>
    /// A <see cref="UnitValue"/> with a constant pre-evaluated value.
    /// </summary>
    public static UnitValue Const { get; } = GetConst();

    private static UnitValue GetConst()
    {
        var x = new UnitValue(() => Unit.Value);
        x.Eval();
        return x;
    }

    protected override string? DisplayValue(Unit value) => "()";
}
