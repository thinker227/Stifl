using System.Diagnostics.CodeAnalysis;
using OneOf;

namespace Stifl;

internal class OneOfEqualityComparer<T1, T2>(
    IEqualityComparer<T1> t1Comparer,
    IEqualityComparer<T2> t2Comparer)
    : IEqualityComparer<OneOf<T1, T2>>
{
    public bool Equals(OneOf<T1, T2> x, OneOf<T1, T2> y) => (x.Index, y.Index) switch
    {
        (0, 0) => t1Comparer.Equals(x.AsT0, y.AsT0),
        (1, 1) => t2Comparer.Equals(x.AsT1, y.AsT1),
        _ => false,
    };

    public int GetHashCode([DisallowNull] OneOf<T1, T2> obj) => obj.Match(
        t1 => t1 is not null
            ? t1Comparer.GetHashCode(t1)
            : 0,
        t2 => t2 is not null
            ? t2Comparer.GetHashCode(t2)
            : 0);
}
