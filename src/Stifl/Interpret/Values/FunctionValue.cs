using Stifl.Types;

namespace Stifl.Interpret;

/// <summary>
/// A function value.
/// </summary>
/// <param name="type">The type of the function.</param>
/// <param name="eval">A function to evaluate the value.</param>
public sealed class FunctionValue(FuncType type, Func<Ast.Expr> eval)
    : Value<Ast.Expr, FuncType>(type, eval)
{
    /// <summary>
    /// Creates a <see cref="FunctionValue"/> with a constant pre-evaluated value.
    /// </summary>
    /// <param name="type">The type of the function.</param>
    /// <param name="func">The constant pre-evaluated value.</param>
    public static FunctionValue Const(FuncType type, Ast.Expr value)
    {
        var x = new FunctionValue(type, () => value);
        x.Eval();
        return x;
    }
}
