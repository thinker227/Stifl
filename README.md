# Small Type-Inferred Functional Language

Simple ML-inspired purely functional language with [Hindley Milner](https://en.wikipedia.org/wiki/Hindley%E2%80%93Milner_type_system) type inferrence.

## Language features
- Functions
  - Functions may only take one argument.
  - `fn x => y`
  - Top-level functions are anonymous functions bound to a name.
    - `f = fn x => ...;`
  - Implicit generalized bindings
- Function invocation
  - `f x`
  - Currying!
    - `f x y` = `(f x) y`
- `if`-`then`-`else` expressions
  - `if cond then ifTrue else ifFalse`
- `let` bindings
  - `let x = y in z`
- Tuples
  - `(a, b, c)`
- Lists
  - `[a, b, c]`
- Undefined
  - `?`
  - Throws an error at runtime.
  - Only natural expression with bottom type as result.
- Types
  - 32-bit integers
  - Booleans
  - Functions (`t1 -> t2`)
  - Tuples (`(t1, t2, t3)`)
  - Lists (`[t]`)
  - Type variables (`'t`)
  - Unit (`()`)
  - Bottom (`_`)
    - Unifies with everything.
    - Expressions may not manually be typed with `_`.
    - May only be inferred from usage of `?`.
- Type annotations
  - `x: T`
- Type inferrence
  - `f = fn x => x;` => `f: 'a -> 'a`
  - `f = fn isZero => if isZero 1 then 2 else 3;` => `f: (Int -> Bool) -> Int`

## Todo
- [x] Parsing
  - [x] AST
  - [ ] Error recovery
- [x] Symbol binding
- [x] Type checking
  - [x] Type annotation
  - [x] Constraint generation
  - [x] Constraint solver
- [ ] Error reporting
- [ ] Interpreter
- [ ] Code generation
  - [ ] Bytecode
