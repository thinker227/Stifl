using Pidgin;
using Stifl.Types;

namespace Stifl.Interpret;

/// <summary>
/// An interpreter for a <see cref="Compilation"/>.
/// </summary>
public sealed class Interpreter
{
    private readonly InterpreterVisitor visitor;

    /// <summary>
    /// The compilation which is being interpreted.
    /// </summary>
    public Compilation Compilation { get; }

    /// <param name="compilation">The compilation to interpret.</param>
    public Interpreter(Compilation compilation)
    {
        visitor = new(this);
        Compilation = compilation;
    }

    /// <summary>
    /// Evaluates an expression.
    /// </summary>
    /// <param name="expr">The expression to evaluate.</param>
    public IValue Evaluate(Ast.Expr expr) => visitor.VisitNode(expr);

    /// <summary>
    /// Evaluates a <see cref="Ast.Expr.IntLiteral"/>.
    /// </summary>
    /// <param name="expr">The <see cref="Ast.Expr.IntLiteral"/> to evaluate.</param>
    public IntValue Evaluate(Ast.Expr.IntLiteral expr) =>
        (IntValue)Evaluate((Ast.Expr)expr);

    /// <summary>
    /// Evaluates a <see cref="Ast.Expr.BoolLiteral"/>.
    /// </summary>
    /// <param name="expr">The <see cref="Ast.Expr.BoolLiteral"/> to evaluate.</param>
    public BoolValue Evaluate(Ast.Expr.BoolLiteral expr) =>
        (BoolValue)Evaluate((Ast.Expr)expr);

    /// <summary>
    /// Evaluates a <see cref="Ast.Expr.Unit"/>.
    /// </summary>
    /// <param name="expr">The <see cref="Ast.Expr.Unit"/> to evaluate.</param>
    public UnitValue Evaluate(Ast.Expr.Unit expr) =>
        (UnitValue)Evaluate((Ast.Expr)expr);

    /// <summary>
    /// Evaluates a <see cref="Ast.Expr.Func"/>.
    /// </summary>
    /// <param name="expr">The <see cref="Ast.Expr.Func"/> to evaluate.</param>
    public FunctionValue Evaluate(Ast.Expr.Func expr) =>
        (FunctionValue)Evaluate((Ast.Expr)expr);

    /// <summary>
    /// Evaluates a <see cref="Ast.Decl.Binding"/>.
    /// </summary>
    /// <param name="binding">The <see cref="Ast.Decl.Binding"/> to evaluate.</param>
    public IValue Evaluate(Ast.Decl.Binding binding) => visitor.VisitNode(binding);

    /// <summary>
    /// Calls a function with a value as its argument.
    /// </summary>
    /// <param name="func">The function to call.</param>
    /// <param name="arg">The argument to the function.</param>
    /// <returns>The return value of the function.</returns>
    public IValue Call(Ast.Expr.Func func, IValue arg)
    {
        var value = Evaluate(func);
        return Call(value, arg);
    }

    /// <summary>
    /// Calls a function with a value as its argument.
    /// </summary>
    /// <param name="function">The function to call.</param>
    /// <param name="arg">The argument to the function.</param>
    /// <returns>The return value of the function.</returns>
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
    private EvaluationContext context = new(null, []);

    private Compilation Compilation => interpreter.Compilation;

    protected override IValue Default => throw new InvalidOperationException();

    public override IValue VisitBindingDecl(Ast.Decl.Binding node)
    {
        var value = VisitNode(node.Expression);
        var symbol = Compilation.SymbolOf(node);

        context.Symbols[symbol] = value;

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

        var parentCtx = context;
        var ctx = new EvaluationContext(parentCtx, []);
        ctx.Symbols[symbol] = varValue;

        context = ctx;
        var value = VisitNode(node.Expression);
        context = parentCtx;

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
        var ctx = context;

        return new FunctionValue(type, () => (node, ctx));
    }

    public override IValue VisitCallExpr(Ast.Expr.Call node)
    {
        var fv = VisitNode(node.Function);
        var function = fv as FunctionValue
            ?? throw new InvalidOperationException(
                $"Cannot call a value of type {fv.Type}.");

        var argument = VisitNode(node.Argument);

        return function.Type.Return switch
        {
            FuncType t => new FunctionValue(t, Call<(Ast.Expr.Func, IEvaluationContext), FunctionValue>()),
            
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

        Func<T> Call<T, TValue>()
            where TValue : class, IValue<T> => () =>
        {
            var (func, funcCtx) = function!.Eval();

            var parameter = Compilation.SymbolOf(func);

            var prevCtx = context;
            var ctx = new EvaluationContext(funcCtx, []);
            ctx.Symbols[parameter] = argument!;

            context = ctx;
            var value = VisitNode(func.Body) as TValue
                ?? throw new InvalidOperationException("Bad value.");
            context = prevCtx;

            return (T)value.Eval()!;
        };
    }

    public override IValue VisitAnnotatedExpr(Ast.Expr.Annotated node) =>
        VisitNode(node.Expression);
}
