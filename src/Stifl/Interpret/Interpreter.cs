using Pidgin;
using Stifl.Types;

namespace Stifl.Interpret;

/// <summary>
/// An interpreter for a <see cref="Compilation"/>.
/// </summary>
/// <param name="compilation">The compilation to interpret.</param>
public sealed class Interpreter
{
    private readonly InterpreterVisitor visitor;

    public Compilation Compilation { get; }

    public Interpreter(Compilation compilation)
    {
        visitor = new(this);
        Compilation = compilation;
    }

    public IValue Evaluate(Ast.Expr expr) => visitor.VisitNode(expr);

    public IntValue Evaluate(Ast.Expr.IntLiteral expr) =>
        (IntValue)Evaluate((Ast.Expr)expr);

    public BoolValue Evaluate(Ast.Expr.BoolLiteral expr) =>
        (BoolValue)Evaluate((Ast.Expr)expr);

    public UnitValue Evaluate(Ast.Expr.Unit expr) =>
        (UnitValue)Evaluate((Ast.Expr)expr);

    public FunctionValue Evaluate(Ast.Expr.Func expr) =>
        (FunctionValue)Evaluate((Ast.Expr)expr);

    public IValue Evaluate(Ast.Decl.Binding binding) => visitor.VisitNode(binding);

    public IValue Call(Ast.Expr.Func func, IValue arg)
    {
        var value = Evaluate(func);
        return Call(value, arg);
    }

    public IValue Call(FunctionValue function, IValue arg)
    {
        if (!arg.Type.Equals(function.Type.Parameter))
            throw new InvalidOperationException(
                $"Cannot call function {function.Type} with argument of type {arg.Type}.");

        throw new NotImplementedException();
    }
}

internal sealed class InterpreterVisitor(Interpreter interpreter) : AstVisitor<IValue>
{
    private readonly Stack<Context> contexts = new([new(null, [])]);

    private Compilation Compilation => interpreter.Compilation;

    protected override IValue Default => throw new InvalidOperationException();

    public override IValue VisitBindingDecl(Ast.Decl.Binding node)
    {
        var value = VisitNode(node.Expression);
        var symbol = Compilation.SymbolOf(node);

        contexts.Peek().Symbols[symbol] = value;

        return value;
    }

    public override IValue VisitUndefinedLiteralExpr(Ast.Expr.UndefinedLiteral node) =>
        throw new InvalidOperationException("Evaluate undefined.");

    public override IValue VisitBoolLiteralExpr(Ast.Expr.BoolLiteral node) =>
        BoolValue.Const(node.Value);

    public override IValue VisitIntLiteralExpr(Ast.Expr.IntLiteral node) =>
        IntValue.Const(node.Value);

    public override IValue VisitUnitExpr(Ast.Expr.Unit node) =>
        UnitValue.Const;

    public override IValue VisitIdentifierExpr(Ast.Expr.Identifier node)
    {
        var symbol = Compilation.ReferencedSymbolOf(node)!;
        var context = contexts.Peek();
        return context.Lookup(symbol);
    }

    public override IValue VisitIfExpr(Ast.Expr.If node)
    {
        if (VisitNode(node.Condition) is not BoolValue condition)
            throw new InvalidOperationException(
                "If condition has to be a boolean.");

        return condition.Eval()
            ? VisitNode(node.IfTrue)
            : VisitNode(node.IfFalse);
    }

    public override IValue VisitLetExpr(Ast.Expr.Let node)
    {
        var varValue = VisitNode(node.Value);
        var symbol = Compilation.SymbolOf(node);

        var context = new Context(contexts.Peek(), []);
        context.Symbols[symbol] = varValue;

        contexts.Push(context);
        var value = VisitNode(node.Expression);
        contexts.Pop();

        return value;
    }

    public override IValue VisitTupleExpr(Ast.Expr.Tuple node)
    {
        var values = node.Values.Select(VisitNode);
        return TupleValue.FromList(values);
    }

    public override IValue VisitListExpr(Ast.Expr.List node)
    {
        var type = Compilation.TypeOf(node);
        var values = node.Values.Select(VisitNode);
        return ListValue.FromList(type, values)
            ?? throw new InvalidOperationException(
                "List expression has inconsistent types.");
    }

    public override IValue VisitFuncExpr(Ast.Expr.Func node)
    {
        var type = Compilation.TypeOf(node);
        var context = contexts.Peek();

        // TODO: Capture context.
        return new FunctionValue(type, () => node);
    }

    public override IValue VisitCallExpr(Ast.Expr.Call node)
    {
        var fv = VisitNode(node.Function);
        var function = fv as FunctionValue
            ?? throw new InvalidOperationException(
                $"Cannot call a value of type {fv.Type}.");

        var parentContext = contexts.Peek();
        var argument = VisitNode(node.Argument);

        Func<T> Call<T, TValue>()
            where TValue : class, IValue<T> => () =>
        {
            var func = function!.Eval();

            var parameter = Compilation.SymbolOf(func);
            var context = new Context(parentContext, []);
            context.Symbols[parameter] = argument!;

            contexts.Push(context);
            var value = VisitNode(func.Body) as TValue
                ?? throw new InvalidOperationException("Bad value.");
            contexts.Pop();

            return (T)value.Eval()!;
        };

        return function.Type.Return switch
        {
            FuncType t => new FunctionValue(t, Call<Ast.Expr.Func, FunctionValue>()),
            
            ListType t => new ListValue(t, Call<(IValue, ListValue)?, ListValue>()),

            TupleType t => new TupleValue(t, Call<ImmutableArray<IValue>, TupleValue>()),

            WellKnownType { Kind: WellKnownTypeKind.Bool } =>
                new BoolValue(Call<bool, BoolValue>()),

            WellKnownType { Kind: WellKnownTypeKind.Int } =>
                new IntValue(Call<int, IntValue>()),

            WellKnownType { Kind: WellKnownTypeKind.Unit } =>
                new UnitValue(Call<Unit, UnitValue>()),

            _ => throw new NotImplementedException(),
        };
    }

    public override IValue VisitAnnotatedExpr(Ast.Expr.Annotated node) =>
        VisitNode(node.Expression);
}

internal sealed record Context(Context? Parent, Dictionary<ISymbol, IValue> Symbols)
{
    public IValue Lookup(ISymbol symbol) =>
        Symbols.GetValueOrDefault(symbol)
            ?? Parent?.Lookup(symbol)
                ?? throw new InvalidOperationException(
                    $"No value for symbol {symbol} exists.");
}
