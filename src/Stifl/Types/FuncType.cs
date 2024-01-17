namespace Stifl.Types;

/// <summary>
/// A function type.
/// </summary>
/// <param name="Parameter">The parameter type of the function.</param>
/// <param name="Return">The return type of the function.</param>
public sealed record FuncType(IType Parameter, IType Return) : IType
{
    public IType Substitute<T>(Func<T, bool> predicate, Func<T, IType> sub) where T : IType =>
        TypeExtensions.Sub(this, predicate, sub, x => new FuncType(
            x.Parameter.Substitute(predicate, sub),
            x.Return.Substitute(predicate, sub)));

    public IEnumerable<IType> Children() => [Parameter, Return];

    public override string ToString() =>
        Parameter is FuncType f
            ? $"({f}) -> {Return}"
            : $"{Parameter} -> {Return}";
}
