namespace Schemish {
  /// <summary>
  /// Represents a procedure value in Scheme.
  /// </summary>
  internal interface ICallable {
    Symbol? Identifier { get; }

    /// <summary>
    /// Invokes this procedure.
    /// </summary>
    /// <param name="args">The arguments. These are the `cdr` of the s-expression for the procedure
    /// invocation.</param>
    /// <param name="stack">The call stack.</param>
    /// <returns>the result of the procedure invocation.</returns>
    object? Call(Cons? args, CallStack? stack);
  }
}
