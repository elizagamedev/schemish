namespace Schemish {
  /// <summary>
  /// Represents a procedure value in Schemish.
  /// </summary>
  public interface ICallable {
    /// <summary>
    /// Gets the identifier of the procedure or null if anonymous.
    /// </summary>
    Symbol? Identifier { get; }

    /// <summary>
    /// Invokes this procedure.
    /// </summary>
    /// <param name="args">The arguments. These are the <c>cdr</c> of the s-expression for the
    /// procedure invocation.</param>
    /// <param name="stack">The call stack.</param>
    /// <returns>The result of the procedure invocation.</returns>
    object? Call(Cons? args, CallStack? stack);
  }
}
