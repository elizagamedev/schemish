using System.Collections.Generic;

namespace Schemish {
  /// <summary>
  /// A procedure implemented in Scheme.
  /// </summary>
  /// <seealso cref="ICallable" />
  public class Procedure : ICallable {
    public Procedure(object? parameters, object? body, Environment env, SourceLocation? location) {
      Parameters = parameters;
      Body = body;
      Env = env;
      Location = location;
    }

    public object? Parameters { get; private init; }

    public object? Body { get; private init; }

    public Environment Env { get; private init; }

    public SourceLocation? Location { get; private init; }

    /// <summary>
    /// Invokes this procedure.
    /// </summary>
    /// <remarks>
    /// Implementation note: under normal function invocation scenarios, this method is not used.
    /// Instead, a tail call optimization is used in the interpreter evaluation phase that runs
    /// Scheme functions.
    ///
    /// This method is useful however, in macro expansions, and any other occasions where the tail
    /// call optimization is not (yet) implemented.
    ///
    /// <see cref="Interpreter.Evaluate(object, Environment)"/>.
    /// </remarks>
    /// <param name="args">The arguments.</param>
    /// <param name="stack">The call stack. This may be modified by this function.</param>
    /// <returns>the result of the procedure invocation.</returns>
    public object? Call(Cons? args, List<SourceLocation> stack) {
      // NOTE: This is not needed for regular function invoke after the tail call optimization.
      // a (non-native) procedure is now optimized into evaluating the body under the environment
      // formed by the (params, args). So the `Call` method will never be used.
      return Interpreter.Evaluate(
          Body, Environment.FromVariablesAndValues(Parameters, args, Env), stack);
    }

    public override string ToString() {
      return new Cons(null, Symbol.Lambda,
                      new Cons(null, Parameters, new Cons(null, Body, null))).ToString();
    }
  }
}
