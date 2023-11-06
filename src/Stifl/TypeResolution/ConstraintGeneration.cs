using Pidgin;
using Stifl.Types;

namespace Stifl.TypeResolution;

/// <summary>
/// A type constraint.
/// </summary>
internal abstract record Constraint
{
    /// <summary>
    /// Equality between two types.
    /// </summary>
    public sealed record Eq(IType A, IType B) : Constraint
    {
        public override string ToString() => $"{A} ≡ {B}";
    }

    // Might want more kinds of constraints in the future.
}

internal static partial class ConstraintGeneration
{
    /// <summary>
    /// Generates a set of constraints.
    /// </summary>
    /// <param name="bindings">The node to generate the constraints for.</param>
    /// <param name="types">The types to generate constraints from.</param>
    /// <param name="scopes">The scopes of the nodes in the AST.</param>
    public static IReadOnlySet<Constraint> GenerateConstraints(
        IEnumerable<Ast.Decl.Binding> bindings,
        TypeSet types,
        ScopeSet scopes,
        SymbolSet symbols,
        TypeVariableInator variableInator)
    {
        var constraints = new HashSet<Constraint>();

        // Constraint generation has to begin by generating these generalization builder types
        // so that recursive bindings can reference each other properly.
        // Constraints for the bindings are added at the same time because it's convenient.
        var generalizations = new Dictionary<AstOrSymbol, IType>();
        foreach (var binding in bindings)
        {
            var type = types[binding];
            var symbol = symbols[binding];
            var expressionType = types[binding.Expression];

            var generalization = new GeneralizationBuilder(expressionType);
            generalizations[binding] = generalization;
            generalizations[AstOrSymbol.FromT1(symbol)] = generalization;

            constraints.Add(new Constraint.Eq(type, generalization));
        }
        // var generalizations = bindings
        //     .ToDictionary(
        //         b => AstOrSymbol.FromT1(symbols[b]),
        //         b => (IType)new GeneralizationBuilder(types[b.Expression]));
        var typesWithGeneralizations = generalizations.Union(types);

        foreach (var binding in bindings)
        {
            var visitor = new ConstraintGenerationVisitor(
                typesWithGeneralizations,
                scopes,
                variableInator);
            
            visitor.VisitNode(binding);

            foreach (var c in visitor.constraints) constraints.Add(c);
        }

        return constraints;
    }
}

file sealed class ConstraintGenerationVisitor(
    TypeSet types,
    ScopeSet scopes,
    TypeVariableInator variableInator) : AstVisitor<Unit>
{
    public readonly HashSet<Constraint> constraints = [];
    public GeneralizationBuilder? generalizationBuilder;

    protected override Unit Default => Unit.Value;

    private IType GetType(Ast node) =>
        types[AstOrSymbol.FromT0(node)];

    private IType GetType(ISymbol symbol) =>
        types[AstOrSymbol.FromT1(symbol)];

    private IType ToType(AstType type) => type switch
    {
        AstType.Unit => WellKnownType.Unit,
        AstType.Int => WellKnownType.Int,
        AstType.Bool => WellKnownType.Bool,
        AstType.Func func => new FuncType(
            ToType(func.Parameter),
            ToType(func.Return)),
        AstType.Tuple tuple => new TupleType(tuple.Types.Select(ToType).ToList()),
        AstType.List list => new ListType(ToType(list.Containing)),
        AstType.Var var => scopes[var].LookupTypeParameter(var.Name)
            ?? throw new InvalidOperationException(
                $"Type variable node '{var.Name} does not have an associated symbol."),
        _ => throw new UnreachableException(),
    };

    private void Eq(Ast a, Ast b) =>
        constraints.Add(new Constraint.Eq(GetType(a), GetType(b)));

    private void Eq(Ast node, ISymbol symbol) =>
        constraints.Add(new Constraint.Eq(GetType(node), GetType(symbol)));

    private void Eq(Ast node, IType type) =>
        constraints.Add(new Constraint.Eq(GetType(node), type));

    private void Eq(IType variable, IType type) =>
        constraints.Add(new Constraint.Eq(variable, type));

    public override Unit VisitBindingDecl(Ast.Decl.Binding node)
    {
        // No constraints are generated for the node here
        // because they have already been generated ahead of being visited.

        var expression = GetType(node.Expression);

        if (node.AnnotatedType is not null)
        {
            var annotatedType = ToType(node.AnnotatedType);
            
            Eq(expression, annotatedType);
        }

        // This cast *should* be safe because all bindings should have
        // a generalization builder already registered.
        generalizationBuilder ??= (GeneralizationBuilder)GetType(node);

        VisitNodeOrNull(node.AnnotatedType);
        VisitNode(node.Expression);

        return Default;
    }

    public override Unit VisitBoolLiteralExpr(Ast.Expr.BoolLiteral node)
    {
        Eq(node, WellKnownType.Bool);
        return Default;
    }

    public override Unit VisitIntLiteralExpr(Ast.Expr.IntLiteral node)
    {
        Eq(node, WellKnownType.Int);
        return Default;
    }

    public override Unit VisitUnitExpr(Ast.Expr.Unit node)
    {
        Eq(node, WellKnownType.Unit);
        return Default;
    }

    public override Unit VisitUndefinedLiteralExpr(Ast.Expr.UndefinedLiteral node)
    {
        Eq(node, WellKnownType.Bottom);
        return Default;
    }

    public override Unit VisitIdentifierExpr(Ast.Expr.Identifier node)
    {
        var symbol = scopes[node].Lookup(node.Name)
            ?? throw new InvalidOperationException($"{node.Name} isn't declared");
        
        // Identifiers are the only kinds of expressions which pull type information
        // from possibly other bindings, and the retrieved type therefore has to be
        // instantiated to replace potential generalizations with type variables.
        // Generalization builders may occur here if the currently visited binding
        // is recursive, in which case there should be a generalization builder
        // for the binding.
        var type = GetType(symbol) switch
        {
            GeneralizationBuilder g => g.Containing,
            var t => t.Instantiate(variableInator.Instantiation()),
        };

        Eq(node, type);

        return Default;
    }

    public override Unit VisitFuncExpr(Ast.Expr.Func node)
    {
        var parameterSymbol = scopes[node.Body].Lookup(node.Parameter)
            ?? throw new InvalidOperationException($"{node.Parameter} isn't declared");
        var parameter = GetType(parameterSymbol);

        var body = GetType(node.Body);

        var funcType = new FuncType(parameter, body);
        
        Eq(node, funcType);

        if (node.AnnotatedType is not null)
        {
            var type = ToType(node.AnnotatedType);

            Eq(parameter, type);
        }

        VisitNodeOrNull(node.AnnotatedType);
        VisitNode(node.Body);

        return Default;
    }

    public override Unit VisitIfExpr(Ast.Expr.If node)
    {
        Eq(node.Condition, WellKnownType.Bool);
        Eq(node.IfTrue, node.IfFalse);
        Eq(node, node.IfTrue);

        VisitNode(node.Condition);
        VisitNode(node.IfTrue);
        VisitNode(node.IfFalse);

        return Default;
    }

    public override Unit VisitCallExpr(Ast.Expr.Call node)
    {
        var argument = GetType(node.Argument);
        var @return = GetType(node);
        var funcType = new FuncType(argument, @return);

        Eq(node.Function, funcType);

        VisitNode(node.Function);
        VisitNode(node.Argument);

        return Default;
    }

    public override Unit VisitLetExpr(Ast.Expr.Let node)
    {
        var symbol = scopes[node.Expression].Lookup(node.Name)
            ?? throw new InvalidOperationException($"{node.Name} isn't declared");
        var var = GetType(symbol);

        Eq(node.Value, var);

        if (node.AnnotatedType is not null)
        {
            var type = ToType(node.AnnotatedType);

            Eq(var, type);
            Eq(node, type);
        }
        else Eq(node, node.Expression);

        VisitNode(node.Value);
        VisitNode(node.Expression);

        return Default;
    }

    public override Unit VisitTupleExpr(Ast.Expr.Tuple node)
    {
        var types = node.Values
            .Select(GetType)
            .ToImmutableArray();

        var type = new TupleType(types);

        Eq(node, type);

        VisitMany(node.Values).Enumerate();

        return Default;
    }

    public override Unit VisitListExpr(Ast.Expr.List node)
    {
        var containing = variableInator.Next();

        foreach (var value in node.Values)
        {
            var t = GetType(value);
            Eq(t, containing);
        }

        var type = new ListType(containing);

        Eq(node, type);

        VisitMany(node.Values).Enumerate();

        return Default;
    }

    public override Unit VisitAnnotatedExpr(Ast.Expr.Annotated node)
    {
        var annotated = ToType(node.Annotation);

        Eq(node, annotated);
        Eq(node.Expression, annotated);

        VisitNode(node.Expression);
        VisitNode(node.Annotation);

        return Default;
    }

    public override Unit VisitVarType(AstType.Var node)
    {
        var symbol = (TypeParameter)ToType(node);
        generalizationBuilder?.ForallTypes.Add(symbol);

        return Default;
    }
}
