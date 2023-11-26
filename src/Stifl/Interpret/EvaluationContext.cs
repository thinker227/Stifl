namespace Stifl.Interpret;

/// <summary>
/// A context for expression evaluation.
/// </summary>
/// <param name="Parent">The parent context,
/// or <see langword="null"/> if the context is the root context.</param>
/// <param name="Symbols">The symbols in the context.</param>
internal sealed record class EvaluationContext(
    IEvaluationContext? Parent,
    Dictionary<ISymbol, IValue> Symbols)
    : IEvaluationContext
{
    public IValue Lookup(ISymbol symbol) =>
        Symbols.GetValueOrDefault(symbol)
            ?? Parent?.Lookup(symbol)
                ?? throw new InvalidOperationException(
                    $"No value for symbol {symbol} exists.");
}

/// <summary>
/// A context for expression evaluation.
/// </summary>
public interface IEvaluationContext
{
    /// <summary>
    /// Gets the value of a symbol.
    /// </summary>
    /// <param name="symbol">The symbol to get the value of.</param>
    IValue Lookup(ISymbol symbol);
}
