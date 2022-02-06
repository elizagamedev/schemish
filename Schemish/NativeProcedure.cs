using System;
using Schemish.Exceptions;

namespace Schemish {
  /// <summary>
  /// A procedure implemented in .NET.
  /// </summary>
  /// <seealso cref="ICallable" />
  public sealed class NativeProcedure : ICallable {
    private readonly Func<Cons?, CallStack?, object?> _func;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeProcedure"/> class.
    /// </summary>
    /// <param name="identifier">The identifier of the procedure.</param>
    /// <param name="func">The native procedure. It accepts a <see cref="Cons"/> list of arguments
    /// and a call stack and returns an object.</param>
    public NativeProcedure(Symbol? identifier,
                           Func<Cons?, CallStack?, object?> func) {
      Identifier = identifier;
      _func = func;
    }

    /// <inheritdoc/>
    public Symbol? Identifier { get; private init; }

    /// <inheritdoc/>
    public object? Call(Cons? args, CallStack? stack) {
      try {
        return _func(args, stack);
      } catch (Exception e) {
        throw new RuntimeErrorException($"Exception during native call. {e.Message}", e,
                                        stack);
      }
    }

    /// <inheritdoc/>
    public override string ToString() {
      if (Identifier is null) {
        return $"#<unknown native procedure>";
      } else {
        return $"#<procedure {Identifier}>";
      }
    }
  }
}
