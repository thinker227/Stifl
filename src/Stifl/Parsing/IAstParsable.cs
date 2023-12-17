using Pidgin;
using Stifl.Parsing;

namespace Stifl.Parsing
{
    /// <summary>
    /// Defines an AST node which has a corresponding parser.
    /// </summary>
    /// <typeparam name="TSelf">The self type.</typeparam>
    public interface IAstParsable<TSelf> where TSelf : IAstParsable<TSelf>
    {
        /// <summary>
        /// The <see cref="Parser{TToken, T}"/> which parses the node.
        /// </summary>
        static abstract Parser<char, TSelf> Parser { get; }
    }
}

namespace Stifl
{
    public partial record Ast
    {
        public partial record Unit : IAstParsable<Unit>
        {
            public static Parser<char, Unit> Parser => Parse.Unit;
        }

        public partial record Decl : IAstParsable<Decl>
        {
            public static Parser<char, Decl> Parser => Parse.Decl;

            public partial record Binding : IAstParsable<Binding>
            {
                public static new Parser<char, Binding> Parser => Parse.Binding;
            }
        }

        public partial record Expr : IAstParsable<Expr>
        {
            public static Parser<char, Expr> Parser => Parse.Expr;
        }
        
        public partial record Type : IAstParsable<Type>
        {
            public static Parser<char, Type> Parser => Parse.Type;
        }
    }
}
