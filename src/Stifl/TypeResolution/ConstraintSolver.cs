using Stifl.Types;

namespace Stifl.TypeResolution;

internal static class ConstraintSolver
{
    /// <summary>
    /// Solves a set of constraints.
    /// </summary>
    /// <param name="annotations">The annotations for which to solve the constraints.</param>
    /// <param name="constraints">The constraints to solve.</param>
    /// <param name="unsolved">A function which handles an unsolved type variable.</param>
    /// <returns>The solution for the constraints.</returns>
    public static TypeSet Solve(
        AnnotationSet annotations,
        IReadOnlySet<Constraint> constraints)
    {
        var solver = new Solver();
        solver.Solve(constraints);

        // Substitute unsolved type variables with type parameters.
        var unsolved = annotations.Values.SelectMany(GetUnsolvedVariables).Distinct();
        foreach (var variable in unsolved)
        {
            // TODO: Generate better names for generated type parameters.
            var param = new TypeParameter(variable.ToString());

            variable.Substitute(new TypeParameter($"unsolved_{variable}"));
        }

        // Purify type variables into their substitutions.
        var types = new Dictionary<AstOrSymbol, IType>();
        foreach (var (x, variable) in annotations)
        {
            var type = variable.Purify();
            
            // All unsolved type variables should have been solved by the previous step.
            // Otherwise, this is probably a bug.
            if (type is TypeVariable) throw new InvalidOperationException(
                $"Unexpected type variable {type} remaining after having tried " +
                "to solve all unsolved type variables after unification.");

            types[x] = type;
        }
        
        return types;
    }

    private static IEnumerable<TypeVariable> GetUnsolvedVariables(IType type) =>
        // Type variables without a substitution will by definition
        // not have any containing types, they're terminal.
        type is TypeVariable { HasSustitution: false } var
            ? [var]
            : type.ContainedTypes()
                .SelectMany(GetUnsolvedVariables);
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

    private static IType Lower(IType t) =>
        t is TypeVariable var
            ? var.Substitution
            : t;

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

        case (WellKnownType wa, WellKnownType wb):
            // Bottom unifies with everything.
            if (wa.Kind == WellKnownTypeKind.Bottom || wb.Kind == WellKnownTypeKind.Bottom) break;

            if (wa != wb) goto default;
            
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
