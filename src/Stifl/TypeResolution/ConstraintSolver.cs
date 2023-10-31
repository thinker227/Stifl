using Stifl.Types;

namespace Stifl.TypeResolution;

internal static class ConstraintSolver
{
    /// <summary>
    /// Solves a set of constraints by substituting the types variables in the constraints.
    /// </summary>
    /// <param name="constraints">The constraints to solve.</param>
    public static void Solve(IReadOnlySet<Constraint> constraints)
    {
        var solver = new Solver();
        solver.Solve(constraints);
    }
}

file sealed class Solver
{
    public void Solve(IReadOnlySet<Constraint> constraints)
    {
        foreach (var constraint in constraints)
            Solve(constraint);
    }

    private void Solve(Constraint constraint)
    {
        switch (constraint)
        {
        case Constraint.Eq eq:
            Unify(eq.A, eq.B);
            break;

        // Might want more kinds of constraints in the future.
        }
    }

    private bool Occurs(IType a, IType b)
    {
        static bool Occurs(IType occurs, IType within)
        {
            var contained = within.ContainedTypes();

            if (contained.Contains(occurs)) throw new InvalidOperationException(
                $"Cannot construct infinite type {occurs} ~ {within}.");

            return false;
        }

        return Occurs(a, b) || Occurs(b, a);
    }

    private static IType Lower(IType t) => t switch
    {
        TypeVariable var => var.Substitution,
        GeneralizationBuilder builder => builder.Containing,
        _ => t, 
    };

    private void Unify(IType a, IType b)
    {
        a = Lower(a);
        b = Lower(b);

        switch (a, b)
        {
        // Type variables unify with everything,
        // as long as they pass the occurs check.
        case (TypeVariable va, _):
            if (!Occurs(va, b)) va.Substitute(b);
            break;
        case (_, TypeVariable vb):
            if (!Occurs(a, vb)) vb.Substitute(a);
            break;

        // Bottom unifies with everything.
        case (WellKnownType { Kind: WellKnownTypeKind.Bottom }, _):
            break;
        case (_, WellKnownType { Kind: WellKnownTypeKind.Bottom }):
            break;

        case (FuncType fa, FuncType fb):
            Unify(fa.Parameter, fb.Parameter);
            Unify(fa.Return, fb.Return);
            break;

        default:
            if (!a.Equals(b))
                throw new InvalidOperationException($"Cannot unify {a} and {b}.");
            break;
        }
    }
}
