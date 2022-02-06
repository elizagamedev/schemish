using System;
using Schemish.Exceptions;

namespace Schemish {
  /// <summary>
  /// A procedure implemented in .NET.
  /// </summary>
  /// <seealso cref="ICallable" />
  public sealed class NativeProcedure : ICallable {
    private readonly Func<Cons?, CallStack?, object?> _func;

    public NativeProcedure(Symbol? identifier,
                           Func<Cons?, CallStack?, object?> func) {
      Identifier = identifier;
      _func = func;
    }

    public Symbol? Identifier { get; private init; }

    public object? Call(Cons? args, CallStack? stack) {
      try {
        return _func(args, stack);
      } catch (Exception e) {
        throw new RuntimeErrorException($"Exception during native call. {e.Message}", e,
                                        stack);
      }
    }

    public override string ToString() {
      if (Identifier is null) {
        return $"#<unknown native procedure>";
      } else {
        return $"#<procedure {Identifier}>";
      }
    }
  }
}
