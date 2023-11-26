namespace Stifl.Interpret;

/// <summary>
/// A context for expression evaluation.
/// </summary>
/// <param name="Parent">The parent context,
/// or <see langword="null"/> if the context is the root context.</param>
/// <param name="Symbols">The symbols in the context.</param>
public sealed record class EvaluationContext(EvaluationContext? Parent, Dictionary<ISymbol, IValue> Symbols)
{
    public IValue Lookup(ISymbol symbol) =>
        Symbols.GetValueOrDefault(symbol)
            ?? Parent?.Lookup(symbol)
                ?? throw new InvalidOperationException(
                    $"No value for symbol {symbol} exists.");
}
