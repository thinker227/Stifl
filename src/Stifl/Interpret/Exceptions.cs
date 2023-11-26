namespace Stifl.Interpret;

/// <summary>
/// An exception which is the result of an error during evaluation. 
/// </summary>
/// <param name="message">A message describing the error.</param>
public sealed class EvaluationException(string message) : Exception(message);

/// <summary>
/// An exception which is the result a runtime type mismatch.
/// </summary>
/// <param name="message">A message describing the error.</param>
public sealed class TypeMismatchException(string message) : Exception(message);
