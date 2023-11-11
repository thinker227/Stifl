namespace Stifl.Types;

/// <summary>
/// A list type.
/// </summary>
/// <param name="Containing">The type of the values in the list.</param>
public sealed record ListType(IType Containing) : IType
{
    public IType Purify() => new ListType(Containing.Purify());

    public IType Instantiate(Func<TypeParameter, TypeVariable> var) =>
        new ListType(Containing.Instantiate(var));

    public IType ReplaceVars(Func<TypeVariable, IType> replace) =>
        new ListType(Containing.ReplaceVars(replace));

    public IEnumerable<IType> Children() => [Containing];

    public override string ToString() => $"[{Containing}]";
}
