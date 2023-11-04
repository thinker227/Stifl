namespace Stifl.Types;

/// <summary>
/// A well-known type.
/// </summary>
/// <param name="Kind">The kind the well-known type is.</param>
public sealed record WellKnownType(WellKnownTypeKind Kind) : IType
{
    /// <summary>
    /// The unit type.
    /// </summary>
    public static WellKnownType Unit { get; } = new WellKnownType(WellKnownTypeKind.Unit);

    /// <summary>
    /// The boolean type.
    /// </summary>
    public static WellKnownType Bool { get; } = new WellKnownType(WellKnownTypeKind.Bool);

    /// <summary>
    /// The 32-bit integer type.
    /// </summary>
    public static WellKnownType Int { get; } = new WellKnownType(WellKnownTypeKind.Int);

    /// <summary>
    /// The bottom type.
    /// </summary>
    public static WellKnownType Bottom { get; } = new WellKnownType(WellKnownTypeKind.Bottom);

    public IType Purify() => this;

    public IType Instantiate(Func<TypeParameter, TypeVariable> var) => this;

    public IType ReplaceVars(Func<TypeVariable, IType> replace) => this;

    public IEnumerable<IType> Children() => [];

    public override string ToString() => Kind switch
    {
        WellKnownTypeKind.Unit => "()",
        WellKnownTypeKind.Bool => "Bool",
        WellKnownTypeKind.Int => "Int",
        WellKnownTypeKind.Bottom => "_",
        _ => throw new UnreachableException(),
    };
}

/// <summary>
/// The kind of a well-known type.
/// </summary>
public enum WellKnownTypeKind
{
    Unit,
    Bool,
    Int,
    Bottom,
}
