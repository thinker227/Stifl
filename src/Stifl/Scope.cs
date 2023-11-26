using System.Diagnostics.CodeAnalysis;
using Pidgin;
using Stifl.Types;

namespace Stifl;

/// <summary>
/// A scope of an AST node.
/// </summary>
/// <param name="parent">The parent scope. <see langword="null"/> for the global scope.</param>
/// <param name="symbols">The symbols <i>declared</i> in the scope.</param>
public sealed class Scope(
    Scope? parent,
    Ast declaringNode,
    IReadOnlyList<Scope> children,
    IReadOnlyDictionary<string, ISymbol> symbols)
{
    /// <summary>
    /// The parent scope. <see langword="null"/> for the global scope.
    /// </summary>
    public Scope? Parent { get; } = parent;

    /// <summary>
    /// The AST node which declares the scope.
    /// </summary>
    public Ast DeclaringNode { get; } = declaringNode;

    /// <summary>
    /// Whether the scope is the global scope.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Parent))]
    public bool IsGlobal => Parent is null;

    /// <summary>
    /// The child scopes of the scope.
    /// </summary>
    public IReadOnlyList<Scope> Children { get; } = children;

    /// <summary>
    /// The descendant scopes and the current scope.
    /// </summary>
    public IEnumerable<Scope> DescendantScopes =>
        Children.SelectMany(s => s.DescendantScopes).Prepend(this);

    /// <summary>
    /// The symbols <i>declared</i> in the scope.
    /// </summary>
    public IReadOnlyDictionary<string, ISymbol> Symbols { get; } = symbols;

    /// <summary>
    /// All symbols accessible within the scope.
    /// </summary>
    public IEnumerable<ISymbol> AccessibleSymbols =>
        (Parent?.AccessibleSymbols ?? []).Concat(Symbols.Values);

    /// <summary>
    /// Tries to look up a symbol with a specified name
    /// in the current scope or any parent scopes.
    /// </summary>
    /// <param name="name">The name of the symbol to look up.</param>
    /// <returns>The found symbol,
    /// or <see langword="null"/> if it could not be found.</returns>
    public ISymbol? Lookup(string name) =>
        Symbols.GetValueOrDefault(name) ?? Parent?.Lookup(name);

    /// <summary>
    /// Tries to look up a symbol with a specified name and type.
    /// </summary>
    /// <typeparam name="TSymbol">The type of the symbol to look up.</typeparam>
    /// <param name="name">The name of the symbol to look up.</param>
    /// <returns>The found symbol,
    /// or <see langword="null"/> if it could not be found.</returns>
    public TSymbol? Lookup<TSymbol>(string name)
        where TSymbol : class, ISymbol =>
        Lookup(name) as TSymbol;

    /// <summary>
    /// Tries to look up a type parameter.
    /// </summary>
    /// <param name="name">The name of the type parameter.
    /// May include the leading <c>'</c> or not.</param>
    /// <returns></returns>
    public TypeParameter? LookupTypeParameter(string name)
    {
        if (!name.StartsWith('\'')) name = $"'{name}";
        return Lookup<TypeParameter>(name);
    }
}

/// <summary>
/// A mutable wrapper around a <see cref="Stifl.Scope"/>.
/// </summary>
/// <param name="Scope">The wrapped scope.</param>
/// <param name="Children">The child scopes of the scope.
/// To have any effect, should be a reference to the same list
/// as <see cref="Scope.Children"/> of <see cref="Scope"/>.</param>
/// <param name="Symbols">The symbols declared in the scope.
/// To have any effect, should be a reference to the same list
/// as <see cref="Scope.Symbols"/> of <see cref="Scope"/>.</param>
internal sealed record MutableScope(
    Scope Scope,
    MutableScope? Parent,
    Ast DeclaringNode,
    List<Scope> Children,
    Dictionary<string, ISymbol> Symbols)
{
    /// <summary>
    /// Creates a new mutable scope.
    /// </summary>
    /// <param name="parent">The parent scope.</param>
    public static MutableScope Create(MutableScope? parent, Ast declaringNode)
    {
        var children = new List<Scope>();
        var symbols = new Dictionary<string, ISymbol>();
        var scope = new Scope(parent?.Scope, declaringNode, children, symbols);
        return new(scope, parent, declaringNode, children, symbols);
    }
}

/// <summary>
/// Methods for resolving scopes.
/// </summary>
public static class Scopes
{
    /// <summary>
    /// Resolves the scopes of a root node and its descendants.
    /// </summary>
    /// <param name="node">The root node.</param>
    /// <returns>A dictionary of nodes and their associated scopes and symbols.</returns>
    public static (ScopeSet, SymbolSet) ResolveScopes(this Ast node)
    {
        var global = MutableScope.Create(null, node);

        var visitor = new ScopeResolutionVisitor(global);
        visitor.VisitNode(node);

        return (visitor.scopes, visitor.symbols);
    }
}

file sealed class ScopeResolutionVisitor(MutableScope global) : AstVisitor<Unit>
{
    private readonly Stack<MutableScope> scopeStack = new([global]);
    public readonly Dictionary<Ast, Scope> scopes = new(ReferenceEqualityComparer.Instance);
    public readonly Dictionary<Ast, ISymbol> symbols = new(ReferenceEqualityComparer.Instance);

    private MutableScope Current => scopeStack.Peek();

    protected override Unit Default => Unit.Value;

    private static MutableScope FindParentFunctionOrGlobalScope(MutableScope scope) =>
        scope.DeclaringNode is Ast.Decl.Binding
            ? scope
            : scope.Parent is null 
                ? scope
                : FindParentFunctionOrGlobalScope(scope.Parent);

    private void Register(ISymbol symbol, Ast associatedNode)
    {
        // If the symbol being registered is a type parameter
        // then it should be registered in the parent binding's scope.
        var scope = symbol is ITypeParameter
            ? FindParentFunctionOrGlobalScope(Current)
            : Current;

        if (scope.Symbols.ContainsKey(symbol.Name) && symbol is not ITypeParameter)
            throw new InvalidOperationException($"Symbol {symbol.Name} declared multiple times.");
            
        scope.Symbols[symbol.Name] = symbol;
        symbols.Add(associatedNode, symbol);
    }

    private void InScope(Ast declaringNode, Action run)
    {
        var scope = MutableScope.Create(Current, declaringNode);
        Current.Children.Add(scope.Scope);

        scopeStack.Push(scope);
        run();
        scopeStack.Pop();
    }

    protected override void BeforeVisit(Ast node) => scopes[node] = Current.Scope;

    public override Unit VisitBindingDecl(Ast.Decl.Binding node)
    {
        Register(new Binding(node.Name, node), node);

        InScope(node, () =>
        {
            VisitNodeOrNull(node.AnnotatedType);
            VisitNode(node.Expression);
        });

        return Default;
    }

    public override Unit VisitFuncExpr(Ast.Expr.Func node)
    {
        InScope(node, () =>
        {
            VisitNodeOrNull(node.AnnotatedType);
            Register(new Parameter(node.Parameter, node), node);
            VisitNode(node.Body);
        });

        return Default;
    }

    public override Unit VisitLetExpr(Ast.Expr.Let node)
    {
        VisitNodeOrNull(node.AnnotatedType);
        VisitNode(node.Value);
        InScope(node, () =>
        {
            Register(new Variable(node.Name, node), node);
            VisitNode(node.Expression);
        });

        return Default;
    }

    public override Unit VisitVarType(AstType.Var node)
    {
        // Type parameter aren't declared in the same way as other symbols.
        // If a type parameter is referenced which isn't declared,
        // it should automatically be declared.
        if (Current.Scope.LookupTypeParameter(node.Name) is not TypeParameter symbol)
            Register(new TypeParameter(node.Name), node);
        else symbols[node] = symbol;
        
        return Default;
    }
}
