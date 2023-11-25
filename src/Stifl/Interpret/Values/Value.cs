using System.Diagnostics.CodeAnalysis;
using Stifl.Types;

namespace Stifl.Interpret;

/// <summary>
/// A runtime value.
/// </summary>
public interface IValue
{
    /// <summary>
    /// The type of the value.
    /// </summary>
    IType Type { get; }

    /// <summary>
    /// Whether the value has been evaluated yet.
    /// </summary>
    bool IsEvaluated { get; }

    /// <summary>
    /// Evaluates the value.
    /// </summary>
    object? Eval();

    /// <summary>
    /// Tries to get the evaluated value.
    /// </summary>
    /// <param name="value">The evaluated value,
    /// or <see langword="default"/> if it has not been evaluated yet.</param>
    /// <returns>Whether the value has been evaluated.</returns>
    bool TryGetValue(out object? value);
}

/// <summary>
/// Abstract helper for values.
/// </summary>
/// <typeparam name="T">The type of an evaluated value.</typeparam>
/// <param name="eval">A function to evaluate the value.</param>
public abstract class Value<T, TType>(TType type, Func<T> eval) : IValue
    where TType : IType
{
    private bool isEvaluated = false;
    private T? value = default;


    public bool IsEvaluated => isEvaluated;

    /// <inheritdoc cref="IValue.Type"/>
    public TType Type { get; } = type;

    IType IValue.Type => Type;

    /// <inheritdoc cref="IValue.Eval"/>
    public T Eval()
    {
        if (!isEvaluated)
        {
            isEvaluated = true;
            value = eval();
        }

        return value!;
    }

    object? IValue.Eval() => Eval();

    /// <inheritdoc cref="IValue.TryGetValue"/>
    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = this.value;
        return isEvaluated;
    }

    bool IValue.TryGetValue(out object? value)
    {
        var s = TryGetValue(out var x);
        value = x;
        return s;
    }

    public override string? ToString() =>
        isEvaluated
            ? DisplayValue(value!)
            : $"unevaled {Type}";

    protected virtual string? DisplayValue(T value) => value?.ToString();
}

public static class ValueExtensions
{
    /// <summary>
    /// Evaluates all values in a collection of values.
    /// </summary>
    /// <param name="values">The values to evaluate.</param>
    /// <returns>A collection of evaluated values.</returns>
    /// <remarks>
    /// This method is lazy and will evaluate the values on demand.
    /// Use <see cref="EvalAllStrict"/> if the values should be evaluated strictly.
    /// </remarks>
    public static IEnumerable<object?> EvalAll(this IEnumerable<IValue> values) =>
        values.Select(x => x.Eval());

    /// <summary>
    /// Strictly evaluates all values in a collection of values.
    /// </summary>
    /// <param name="values">The values to evaluate.</param>
    /// <returns>A collection of evaluated values.</returns>
    /// <remarks>
    /// This method is strict and will evaluate the values immediately.
    /// Use <see cref="EvalAll"/> if the values should be evaluated lazily.
    /// </remarks>
    public static ImmutableArray<object?> EvalAllStrict(this IEnumerable<IValue> values) =>
        values.EvalAll().ToImmutableArray();

    /// <summary>
    /// Gets the evaluated value,
    /// or <see langword="default"/> if the value has not been evaluated.
    /// </summary>
    /// <param name="value">The value to get the evaluated value of.</param>
    public static object? GetValueOrDefault(this IValue value)
    {
        value.TryGetValue(out var x);
        return x;
    }
}

