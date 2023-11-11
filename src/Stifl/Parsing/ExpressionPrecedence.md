# List of expression precedences
> **Note**
> This list is from highest to lowest, meaning a expressions with higher precedence will bind stronger than - or will be evaluated before - ones with lower precedence.

1. - Parentheses
   - Literals
   - Identifiers
   - Tuples
   - Lists
   - `if`
   - `let`
2. - Function invocation
3. - Type annotations

This means the expression `f 1: T` is equivalent to `((f) (1)): T`. `f` and `1` are evaluated first, then `f 1`, then the entire expression is annotated with the type `T`.
