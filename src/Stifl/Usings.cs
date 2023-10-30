global using System.Collections.Immutable;

global using AstType = Stifl.Ast.Type;
global using SysType = System.Type;
global using UnreachableException = System.Diagnostics.UnreachableException;

global using ScopeSet = System.Collections.Generic.IReadOnlyDictionary<Stifl.Ast, Stifl.Scope>;
global using SymbolSet = System.Collections.Generic.IReadOnlyDictionary<Stifl.Ast, Stifl.ISymbol>;
global using AnnotationSet = System.Collections.Generic.IReadOnlyDictionary<OneOf.OneOf<Stifl.Ast, Stifl.ISymbol>, Stifl.Types.TypeVariable>;
global using TypeSet = System.Collections.Generic.IReadOnlyDictionary<OneOf.OneOf<Stifl.Ast, Stifl.ISymbol>, Stifl.Types.IType>;
global using AstOrSymbol = OneOf.OneOf<Stifl.Ast, Stifl.ISymbol>;
