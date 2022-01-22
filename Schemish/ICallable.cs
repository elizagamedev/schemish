using System.Collections.Generic;

namespace Schemish {
  /// <summary>
  /// Represents a procedure value in Scheme.
  /// </summary>
  internal interface ICallable {
    /// <summary>
    /// Invokes this procedure.
    /// </summary>
    /// <param name="args">The arguments. These are the `cdr` of the s-expression for the procedure
    /// invocation.</param>
    /// <param name="stack">The call stack. This will be modified by this function.</param>
    /// <returns>the result of the procedure invocation.</returns>
    object? Call(Cons? args, List<SourceLocation> stack);
  }
}
