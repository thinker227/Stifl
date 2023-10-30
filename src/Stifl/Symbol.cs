namespace Stifl;

/// <summary>
/// A semantic element able to be referenced across nodes.
/// </summary>
public interface ISymbol
{
    /// <summary>
    /// The name of the symbol, used to reference it.
    /// </summary>
    string Name { get; }
}

/// <summary>
/// A symbol for a binding.
/// </summary>
/// <param name="Name">The name of the binding.</param>
/// <param name="Declaration">The binding declaration.</param>
public sealed record Binding(string Name, Ast.Decl.Binding Declaration) : ISymbol;

/// <summary>
/// A symbol for a function parameter.
/// </summary>
/// <param name="Name">The name of the parameter.</param>
/// <param name="Declaration">The declaration of the function which the parameter belongs to.</param>
public sealed record Parameter(string Name, Ast.Expr.Func Declaration) : ISymbol;

/// <summary>
/// A symbol for a variable.
/// </summary>
/// <param name="Name">The name of the variable.</param>
/// <param name="Declaration">The <c>let</c> expression which declares the variable.</param>
public sealed record Variable(string Name, Ast.Expr.Let Declaration) : ISymbol;
