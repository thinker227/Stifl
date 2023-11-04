namespace Stifl;

#pragma warning disable IDE0001

public abstract partial record Ast
{
    public sealed record Unit(ImmutableArray<Decl> Decls) : Ast;

    public abstract record Decl : Ast
    {
        public sealed record Binding(string Name, Type? AnnotatedType, Expr Expression) : Decl;
    }

    public abstract record Expr : Ast
    {
        public new sealed record Unit : Expr;

        public sealed record UndefinedLiteral : Expr;

        public sealed record BoolLiteral(bool Value) : Expr;

        public sealed record IntLiteral(int Value) : Expr;

        public sealed record Identifier(string Name) : Expr;

        public sealed record Func(string Parameter, Type? AnnotatedType, Expr Body) : Expr;

        public sealed record If(Expr Condition, Expr IfTrue, Expr IfFalse) : Expr;

        public sealed record Call(Expr Function, Expr Argument) : Expr;

        public sealed record Let(string Name, Type? AnnotatedType, Expr Value, Expr Expression) : Expr;

        public sealed record Annotated(Expr Expression, Type Annotation) : Expr;
    }

    public abstract record Type : Ast
    {
        public new sealed record Unit : Type;

        public sealed record Int : Type;

        public sealed record Bool : Type;

        public sealed record Func(Type Parameter, Type Return) : Type;

        public sealed record Var(string Name) : Type;
    }
}
