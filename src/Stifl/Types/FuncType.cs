namespace Stifl.Types;

/// <summary>
/// A function type.
/// </summary>
/// <param name="Parameter">The parameter type of the function.</param>
/// <param name="Return">The return type of the function.</param>
public sealed record FuncType(IType Parameter, IType Return) : IType
{
    public IType Purify() => new FuncType(Parameter.Purify(), Return.Purify());

    public IType Instantiate(Func<TypeParameter, TypeVariable> var) => new FuncType(
        Parameter.Instantiate(var),
        Return.Instantiate(var));

    public IEnumerable<IType> ContainedTypes() => [Parameter, Return];

    public override string ToString() =>
        Parameter is FuncType f
            ? $"({f}) -> {Return}"
            : $"{Parameter} -> {Return}";
}
