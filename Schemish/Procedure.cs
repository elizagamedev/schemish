namespace Schemish {
  /// <summary>
  /// A procedure implemented in Scheme.
  /// </summary>
  /// <seealso cref="ICallable" />
  public sealed class Procedure : ICallable {
    /// <summary>
    /// Initializes a new instance of the <see cref="Procedure"/> class.
    /// </summary>
    /// <param name="identifier">The identifier of the procedure.</param>
    /// <param name="parameters">A <see cref="Symbol"/> or <see cref="Cons"/> list of
    /// <see cref="Symbol"/>s representing the argument names.</param>
    /// <param name="body">The procedure body s-expression.</param>
    /// <param name="env">The lexical scope of the procedure.</param>
    /// <param name="location">The location of the lambda s-exp which defines this procedure.
    /// </param>
    public Procedure(Symbol? identifier, object? parameters, object? body, Environment env,
                     SourceLocation location) {
      Identifier = identifier;
      Parameters = parameters;
      Body = body;
      Env = env;
      Location = location;
    }

    /// <inheritdoc/>
    public Symbol? Identifier { get; private init; }

    /// <summary>
    /// Gets the <see cref="Symbol"/> or <see cref="Cons"/> list of <see cref="Symbol"/>s
    /// representing the argument names.
    /// </summary>
    public object? Parameters { get; private init; }

    /// <summary>
    /// Gets the s-expression of the procedure body.
    /// </summary>
    public object? Body { get; private init; }

    /// <summary>
    /// Gets the lexical environment of the procedure.
    /// </summary>
    public Environment Env { get; private init; }

    /// <summary>
    /// Gets the location of the lambda s-exp which defines this procedure.
    /// </summary>
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
    /// See <see cref="Interpreter.Evaluate"/>.
    /// </remarks>
    /// <param name="args">The arguments. These are the <c>cdr</c> of the s-expression for the
    /// procedure invocation.</param>
    /// <param name="stack">The call stack.</param>
    /// <returns>the result of the procedure invocation.</returns>
    public object? Call(Cons? args, CallStack? stack) {
      return Interpreter.Evaluate(
          Body, Environment.FromVariablesAndValues(Parameters, args, Env),
          ToString(), Location, stack);
    }

    /// <inheritdoc/>
    public override string ToString() {
      if (Identifier is null) {
        return $"#<procedure at {Location}>";
      } else {
        return $"#<procedure {Identifier}>";
      }
    }
  }
}
