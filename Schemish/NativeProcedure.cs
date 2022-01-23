using System;
using System.Collections.Generic;

namespace Schemish {
  /// <summary>
  /// A procedure implemented in .NET.
  /// </summary>
  /// <seealso cref="ICallable" />
  public class NativeProcedure : ICallable {
    private readonly Func<Cons?, List<SourceLocation>, object?> _func;

    public NativeProcedure(Symbol identifier,
                           Func<Cons?, List<SourceLocation>, object?> func) {
      Identifier = identifier;
      _func = func;
    }

    public Symbol? Identifier { get; private init; }

    public object? Call(Cons? args, List<SourceLocation> stack) {
      // Add self to location since won't get the chance to in the proc method.
      stack.Add(new SourceLocation(ToString(), 0, 0, "<native>"));
      return _func(args, stack);
    }

    public override string ToString() {
      string id = Identifier?.ToString() ?? "noname";
      return $"#<NativeProcedure:{id}>";
    }
  }
}
