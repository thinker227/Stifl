using Pidgin;
using Stifl.Types;

namespace Stifl;

/// <summary>
/// A collection of syntactic and semantic information about a compilation.
/// </summary>
public sealed class Compilation
{
    private IReadOnlyDictionary<Ast, Ast>? parents;
    private readonly ScopeSet scopes;
    private readonly SymbolSet symbols;
    private readonly TypeSet types;

    /// <summary>
    /// The root node of the compilation.
    /// </summary>
    public Ast.Unit Root { get; }

    /// <summary>
    /// The global scope of the compilation.
    /// </summary>
    public Scope GlobalScope => ScopeOf(Root);

    private Compilation(Ast.Unit root, ScopeSet scopes, SymbolSet symbols, TypeSet types)
    {
        Root = root;
        this.scopes = scopes;
        this.symbols = symbols;
        this.types = types;
    }

    /// <summary>
    /// Parses a source string and creates a compilation from it.
    /// </summary>
    /// <param name="source">The source to parse.</param>
    public static Compilation Create(string source)
    {
        var unit = Parse.Unit.Full().ParseOrThrow(source);
        return Create(unit);
    }

    /// <summary>
    /// Creates a compilation from a root node.
    /// </summary>
    /// <param name="root">The root node in the AST.</param>
    public static Compilation Create(Ast.Unit root)
    {
        var (scopes, symbols) = root.ResolveScopes();
        var types = TypeResolve.ResolveTypes(root, scopes, symbols);
        
        return new(root, scopes, symbols, types);
    }

    /// <summary>
    /// Gets the parent of a node,
    /// or <see langword="null"/> if the node is the root node of the AST.
    /// </summary>
    /// <param name="node">The node to get the parent of.</param>
    /// <remarks>
    /// This method will only return <see langword="null"/> for <see cref="Root"/>.
    /// </remarks>
    public Ast? ParentOf(Ast node)
    {
        parents ??= node.GetParents();
        return parents.GetValueOrDefault(node);
    }

    /// <summary>
    /// Gets the scope of a node.
    /// </summary>
    /// <param name="node">The node to get the scope of.</param>
    public Scope ScopeOf(Ast node) => scopes[node];

    /// <summary>
    /// Gets the referenced symbol of an <see cref="Ast.Expr.Identifier"/>,
    /// or <see langword="null"/> if the identifier references an invalid symbol.
    /// </summary>
    /// <param name="identifier">The <see cref="Ast.Expr.Identifier"/> to get the referenced symbol of.</param>
    public ISymbol? ReferencedSymbolOf(Ast.Expr.Identifier identifier) =>
        ScopeOf(identifier).Lookup(identifier.Name);

    /// <summary>
    /// Gets the symbol declared by a node,
    /// or <see langword="null"/> if the node does not declare a symbol.
    /// </summary>
    /// <param name="node">The node to get the symbol of.</param>
    public ISymbol? SymbolOf(Ast node) => symbols.GetValueOrDefault(node);

    /// <summary>
    /// Gets the symbol declared by a <see cref="Ast.Decl"/>.
    /// </summary>
    /// <param name="decl">The <see cref="Ast.Decl"/> to get the symbol of.</param>
    public ISymbol SymbolOf(Ast.Decl decl) => symbols[decl];

    /// <summary>
    /// Gets the <see cref="Binding"/> declared by a <see cref="Ast.Decl.Binding"/>.
    /// </summary>
    /// <param name="binding">The <see cref="Ast.Decl.Binding"/> to get the <see cref="Binding"/> of.</param>
    public Binding SymbolOf(Ast.Decl.Binding binding) => (Binding)symbols[binding];

    /// <summary>
    /// Gets the <see cref="Parameter"/> declared by a <see cref="Ast.Expr.Func"/>.
    /// </summary>
    /// <param name="func">The <see cref="Ast.Expr.Func"/> to get the <see cref="Parameter"/> of.</param>
    public Parameter SymbolOf(Ast.Expr.Func func) => (Parameter)symbols[func];

    /// <summary>
    /// Gets the <see cref="Variable"/> declared by a <see cref="Ast.Expr.Let"/>.
    /// </summary>
    /// <param name="let">The <see cref="Ast.Expr.Let"/> to get the <see cref="Variable"/> of.</param>
    public Variable SymbolOf(Ast.Expr.Let let) => (Variable)symbols[let];

    /// <summary>
    /// Gets the type of a node,
    /// or <see langword="null"/> if the node does not have a type.
    /// </summary>
    /// <param name="node">The node to get the type of.</param>
    public IType? TypeOf(Ast node) => types.GetValueOrDefault(node);

    /// <summary>
    /// Gets the type of a <see cref="Ast.Decl.Binding"/>.
    /// </summary>
    /// <param name="binding">The <see cref="Ast.Decl.Binding"/> to get the type of.</param>
    public TypeGeneralization TypeOf(Ast.Decl.Binding binding) => (TypeGeneralization)types[binding];

    /// <summary>
    /// Gets the type of an <see cref="Ast.Expr"/>.
    /// </summary>
    /// <param name="expr">The <see cref="Ast.Expr"/> to get the type of.</param>
    public IType TypeOf(Ast.Expr expr) => types[expr];

    /// <summary>
    /// Gets the type of an <see cref="Ast.Expr.Tuple"/>.
    /// </summary>
    /// <param name="tuple">The <see cref="Ast.Expr.Tuple"/> to get the type of.</param>
    public TupleType TypeOf(Ast.Expr.Tuple tuple) => (TupleType)types[tuple];

    /// <summary>
    /// Gets the type of an <see cref="Ast.Expr.List"/>.
    /// </summary>
    /// <param name="list">The <see cref="Ast.Expr.List"/> to get the type of.</param>
    public ListType TypeOf(Ast.Expr.List list) => (ListType)types[list];

    /// <summary>
    /// Gets the type of an <see cref="Ast.Expr.Func"/>.
    /// </summary>
    /// <param name="func">The <see cref="Ast.Expr.Func"/> to get the type of.</param>
    public FuncType TypeOf(Ast.Expr.Func func) => (FuncType)types[func];

    /// <summary>
    /// Gets the type of a symbol,
    /// or <see langword="null"/> if the symbol does not have a type.
    /// </summary>
    /// <param name="symbol">The symbol to get the type of.</param>
    public IType? TypeOf(ISymbol symbol) => types.GetValueOrDefault(AstOrSymbol.FromT1(symbol));

    /// <summary>
    /// Gets the type of a <see cref="Binding"/>.
    /// </summary>
    /// <param name="binding">The <see cref="Binding"/> to get the type of.</param>
    public TypeGeneralization TypeOf(Binding binding) => (TypeGeneralization)types[binding];

    /// <summary>
    /// Gets the type of a <see cref="Parameter"/>.
    /// </summary>
    /// <param name="parameter">The <see cref="Parameter"/> to get the type of.</param>
    public IType TypeOf(Parameter parameter) => types[parameter];

    /// <summary>
    /// Gets the type of a <see cref="Variable"/>.
    /// </summary>
    /// <param name="variable">The <see cref="Variable"/> to get the type of.</param>
    public IType TypeOf(Variable variable) => types[variable];
}
