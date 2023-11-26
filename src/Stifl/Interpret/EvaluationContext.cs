namespace Stifl.Interpret;

/// <summary>
/// A context for expression evaluation.
/// </summary>
/// <param name="Interpreter">The interpreter the context is for.</param>
/// <param name="Parent">The parent context,
/// or <see langword="null"/> if the context is the root context.</param>
/// <param name="Symbols">The symbols in the context.</param>
internal sealed record EvaluationContext(
    Interpreter Interpreter,
    IEvaluationContext? Parent,
    Dictionary<ISymbol, IValue> Symbols)
    : IEvaluationContext
{
    public IValue? Lookup(ISymbol symbol)
    {
        // Check whether the value already exists.
        if (Symbols.TryGetValue(symbol, out var value)) return value;

        // Look in the parent to try find the value. 
        if (Parent?.Lookup(symbol) is { } parentValue) return parentValue;
        
        if (symbol is not Binding binding) return null;
        
        // If the symbol is a binding, evaluate it.
        var evaluated = Interpreter.Evaluate(binding);
        Symbols[binding] = evaluated;
        return evaluated;
    }
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
    IValue? Lookup(ISymbol symbol);
}
