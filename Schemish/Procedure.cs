namespace Schemish {
  /// <summary>
  /// A procedure implemented in Scheme.
  /// </summary>
  /// <seealso cref="ICallable" />
  public sealed class Procedure : ICallable {
    public Procedure(Symbol? identifier, object? parameters, object? body, Environment env,
                     SourceLocation location) {
      Identifier = identifier;
      Parameters = parameters;
      Body = body;
      Env = env;
      Location = location;
    }

    public Symbol? Identifier { get; private init; }

    public object? Parameters { get; private init; }

    public object? Body { get; private init; }

    public Environment Env { get; private init; }

    public SourceLocation Location { get; private init; }

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
    /// <param name="stack">The call stack.</param>
    /// <returns>the result of the procedure invocation.</returns>
    public object? Call(Cons? args, CallStack? stack) {
      // NOTE: This is not needed for regular function invoke after the tail call optimization.
      // a (non-native) procedure is now optimized into evaluating the body under the environment
      // formed by the (params, args). So the `Call` method will never be used.
      return Interpreter.Evaluate(
          Body, Environment.FromVariablesAndValues(Parameters, args, Env),
          ToString(), Location, stack);
    }

    public override string ToString() {
      if (Identifier is null) {
        return $"#<procedure at {Location}>";
      } else {
        return $"#<procedure {Identifier}>";
      }
    }
  }
}
