using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Schemish.Exceptions;
using static System.Diagnostics.Debug;
using static Schemish.Utils;

namespace Schemish {
  /// <summary>
  /// Contains definitions for native built-in functions.
  /// </summary>
  internal static class Builtins {
    /// <summary>
    /// Creates a dictionary which contains all of the built-in functions for use in
    /// <see cref="Environment"/>.
    /// </summary>
    /// <param name="interpreter">The interpreter under which the builtins will be created.</param>
    /// <returns>
    /// An <see cref="IDictionary{TKey,TValue}"/>for use in an <see cref="Environment"/>.
    /// </returns>
    internal static IDictionary<Symbol, object?> CreateBuiltins(Interpreter interpreter) {
      var procedures = new List<NativeProcedure> {
        new NativeProcedure(Symbol.Intern("+"), AddImpl),
        new NativeProcedure(Symbol.Intern("-"), SubtractImpl),
        new NativeProcedure(Symbol.Intern("*"), MultiplyImpl),
        new NativeProcedure(Symbol.Intern("/"), DivideImpl),

        new NativeProcedure(Symbol.Intern("="), NumEqImpl),
        new NativeProcedure(Symbol.Intern("<"), NumLtImpl),
        new NativeProcedure(Symbol.Intern("<="), NumLeImpl),
        new NativeProcedure(Symbol.Intern(">"), NumGtImpl),
        new NativeProcedure(Symbol.Intern(">="), NumGeImpl),

        new NativeProcedure(Symbol.Intern("eq?"), EqPImpl),
        new NativeProcedure(Symbol.Intern("equal?"), EqualPImpl),
        new NativeProcedure(Symbol.Intern("not"), NotImpl),

        new NativeProcedure(Symbol.Intern("boolean?"), (args, stack) => TypePImpl<bool>(args)),
        new NativeProcedure(Symbol.Intern("num?"),
                            (args, stack) => TypePImpl<int>(args) || TypePImpl<double>(args)),
        new NativeProcedure(Symbol.Intern("string?"), (args, stack) => TypePImpl<string>(args)),
        new NativeProcedure(Symbol.Intern("symbol?"), (args, stack) => TypePImpl<Symbol>(args)),
        new NativeProcedure(Symbol.Intern("list?"), ListPImpl),
        new NativeProcedure(Symbol.Intern("null?"), NullPImpl),

        new NativeProcedure(Symbol.Intern("map"), MapImpl),
        new NativeProcedure(Symbol.Intern("range"), RangeImpl),
        new NativeProcedure(Symbol.Intern("apply"), ApplyImpl),

        new NativeProcedure(Symbol.Intern("list"), (args, stack) => args),
        new NativeProcedure(Symbol.Intern("list-ref"), ListRefImpl),
        new NativeProcedure(Symbol.Intern("length"), LengthImpl),

        new NativeProcedure(Symbol.Intern("car"), CarImpl),
        new NativeProcedure(Symbol.Intern("cdr"), CdrImpl),
        new NativeProcedure(Symbol.Intern("cons"), ConsImpl),
        new NativeProcedure(Symbol.Append, AppendImpl),
        new NativeProcedure(Symbol.Intern("reverse"), ReverseImpl),

        new NativeProcedure(Symbol.Intern("string-length"), StringLengthImpl),
        new NativeProcedure(Symbol.Intern("string-append"), StringAppendImpl),

        new NativeProcedure(Symbol.Intern("string->number"), StringToNumberImpl),
        new NativeProcedure(Symbol.Intern("number->string"), NumberToStringImpl),

        new NativeProcedure(Symbol.Intern("display"),
                            (args, stack) => DisplayImpl(interpreter, args)),
        new NativeProcedure(Symbol.Intern("write"),
                            (args, stack) => WriteImpl(interpreter, args)),
        new NativeProcedure(Symbol.Intern("newline"),
                            (args, stack) => NewlineImpl(interpreter, args)),

        new NativeProcedure(Symbol.Intern("error"), ErrorImpl),
        new NativeProcedure(Symbol.Intern("load"), (args, stack) => LoadImpl(interpreter, args)),
      };

      return procedures.ToDictionary(x => x.Identifier.AsNotNull(), x => (object?)x);
    }

    private static object? AddImpl(Cons? args, CallStack? stack) {
      bool isInt = true;
      double total = 0.0;
      foreach (object? car in args.AsCars()) {
        switch (car) {
          case int i:
            total += i;
            break;
          case double d:
            isInt = false;
            total += d;
            break;
          default:
            throw SchemishException.IllegalConversion(car, "number");
        }
      }
      if (isInt) {
        return (int)total;
      }
      return total;
    }

    private static object? SubtractImpl(Cons? args, CallStack? stack) {
      if (args is null) {
        throw SchemishException.IncorrectArity(0, "at least 2");
      }
      Assert(args.IsList, "args is not a list");

      bool isInt = true;
      double total;
      switch (args.Car) {
        case int i:
          total = i;
          break;
        case double d:
          isInt = false;
          total = d;
          break;
        default:
          throw SchemishException.IllegalConversion(args.Car, "number");
      }

      // (-) with a single argument negates instead of subtracting.
      var head = (Cons?)args.Cdr;
      if (head is null) {
        if (isInt) {
          return (int)-total;
        }
        return -total;
      }

      while (head is not null) {
        switch (head.Car) {
          case int i:
            total -= i;
            break;
          case double d:
            isInt = false;
            total -= d;
            break;
          default:
            throw SchemishException.IllegalConversion(head.Car, "number");
        }
        head = (Cons?)head.Cdr;
      }
      if (isInt) {
        return (int)total;
      }
      return total;
    }

    private static object? MultiplyImpl(Cons? args, CallStack? stack) {
      bool isInt = true;
      double total = 1.0;
      foreach (object? car in args.AsCars()) {
        switch (car) {
          case int i:
            total *= i;
            break;
          case double d:
            isInt = false;
            total *= d;
            break;
          default:
            throw SchemishException.IllegalConversion(car, "number");
        }
      }
      if (isInt) {
        return (int)total;
      }
      return total;
    }

    private static object? DivideImpl(Cons? args, CallStack? stack) {
      if (args is null) {
        throw SchemishException.IncorrectArity(0, "at least 2");
      }
      Assert(args.IsList, "args is not a list");

      double total = args.Car switch {
        int i => i,
        double d => d,
        _ => throw SchemishException.IllegalConversion(args.Car, "number"),
      };

      // (/) with a single argument finds inverse instead of dividing.
      var head = (Cons?)args.Cdr;
      if (head is null) {
        return 1.0 / total;
      }

      while (head is not null) {
        total /= head.Car switch {
          int i => i,
          double d => d,
          _ => throw SchemishException.IllegalConversion(head.Car, "number"),
        };
        head = (Cons?)head.Cdr;
      }
      return total;
    }

    private static object? NumEqImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count == 1) {
        return true;
      }

      double target = ConvertToDouble(args.Car);
      return args.AsCars().Skip(1).All(x => ConvertToDouble(x) == target);
    }

    private static object? NumLtImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count == 1) {
        return true;
      }

      double target = ConvertToDouble(args.Car);

      foreach (object? car in args.AsCars().Skip(1)) {
        double val = ConvertToDouble(car);
        if (val <= target) {
          return false;
        }
        target = val;
      }

      return true;
    }

    private static object? NumLeImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count == 1) {
        return true;
      }

      double target = ConvertToDouble(args.Car);

      foreach (object? car in args.AsCars().Skip(1)) {
        double val = ConvertToDouble(car);
        if (val < target) {
          return false;
        }
        target = val;
      }

      return true;
    }

    private static object? NumGtImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count == 1) {
        return true;
      }

      double target = ConvertToDouble(args.Car);

      foreach (object? car in args.AsCars().Skip(1)) {
        double val = ConvertToDouble(car);
        if (val >= target) {
          return false;
        }
        target = val;
      }

      return true;
    }

    private static object? NumGeImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count == 1) {
        return true;
      }

      double target = ConvertToDouble(args.Car);

      foreach (object? car in args.AsCars().Skip(1)) {
        double val = ConvertToDouble(car);
        if (val > target) {
          return false;
        }
        target = val;
      }

      return true;
    }

    private static object? EqPImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count == 1) {
        return true;
      }
      var cars = args.AsCars();
      object? first = cars.First();
      return cars.Skip(1).All(x => ReferenceEquals(first, x));
    }

    private static object? EqualPImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count == 1) {
        return true;
      }
      var cars = args.AsCars();
      object? first = cars.First();
      return cars.Skip(1).All(x => Equals(first, x));
    }

    private static object? NotImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count != 1) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "1");
      }
      return !ConvertToBool(args.Car);
    }

    private static bool TypePImpl<T>(Cons? args) {
      if (args is null || args.Count != 1) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "1");
      }
      return args.Car is T;
    }

    private static object? ListPImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count != 1) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "1");
      }
      if (args.Car is null) {
        return true;
      }
      if (args.Car is Cons cons) {
        return cons.IsList;
      }
      return false;
    }

    private static object? NullPImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count != 1) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "1");
      }
      return args.Car is null;
    }

    private static object? MapImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count < 2) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "at least 2");
      }
      var cars = args.AsCars();
      var proc = EnsureIsProc(cars.First());

      var lists = cars.Skip(1).Select(x => EnsureIsList(x)).ToList();

      if (lists.All(x => x is null)) {
        return null;
      }

      // Ensure the lists are all the same length.
      int count = lists[0]?.Count ?? 0;
      if (lists.Skip(1).Any(x => (x?.Count ?? 0) != count)) {
        throw new SchemishException("List arguments to map must all be the same size.");
      }

      var enumerators = lists.Select(x => x.AsCars().GetEnumerator()).ToList();

      var result = new List<object?>(count);
      for (int i = 0; i < count; i++) {
        var mapArgs = Cons.CreateFromCars(enumerators.Select(x => x.Take()));
        result.Add(proc.Call(mapArgs, new CallStack("#<procedure call>", SourceLocation.Native,
                                                    stack)));
      }
      return Cons.CreateFromCars(result);
    }

    private static object? RangeImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count is < 1 or > 3) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "1 to 3");
      }

      var argsEnum = args.AsCars().Select(x => EnsureIsInt(x)).GetEnumerator();

      int start, end, step;
      if (args.Count == 1) {
        start = 0;
        end = argsEnum.Take();
        step = 1;
      } else if (args.Count == 2) {
        start = argsEnum.Take();
        end = argsEnum.Take();
        step = 1;
      } else {
        start = argsEnum.Take();
        end = argsEnum.Take();
        step = argsEnum.Take();
      }

      if ((start < end && step <= 0) || (start > end && step >= 0)) {
        throw new SchemishException("Step must make the sequence end.");
      }

      var res = new List<object>();

      if (start <= end) {
        for (int i = start; i < end; i += step) {
          res.Add(i);
        }
      } else {
        for (int i = start; i > end; i += step) {
          res.Add(i);
        }
      }

      return Cons.CreateFromCars(res);
    }

    private static object? ApplyImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count < 2) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "at least 2");
      }

      object? procObject = args.Car;
      if (procObject is not ICallable proc) {
        throw SchemishException.WrongType(procObject, "procedure");
      }
      var procArgs = (Cons?)AppendImpl((Cons?)args.Cdr, stack);
      return proc.Call(procArgs, new CallStack("#<procedure apply>", SourceLocation.Native, stack));
    }

    private static object? ListRefImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count != 2) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "2");
      }

      var argsEnum = args.AsCars().GetEnumerator();

      var list = EnsureIsList(argsEnum.Take());
      int index = EnsureIsInt(argsEnum.Take());

      if (list is null || index < 0 || index > list.Count) {
        throw new SchemishException(
            $"List index {index} out of range of list size {list?.Count ?? 0}.");
      }

      return list.AsCars().ElementAt(index);
    }

    private static object? LengthImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count != 1) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "1");
      }

      return EnsureIsList(args.Car)?.Count ?? 0;
    }

    private static object? CarImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count != 1) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "1");
      }

      return EnsureIsPair(args.Car).Car;
    }

    private static object? CdrImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count != 1) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "1");
      }

      return EnsureIsPair(args.Car).Cdr;
    }

    private static object? ConsImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count != 2) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "2");
      }

      var argsEnum = args.AsCars().GetEnumerator();

      object? head = argsEnum.Take();
      object? tail = argsEnum.Take();

      return new Cons(SourceLocation.Unknown, head, tail);
    }

    private static object? AppendImpl(Cons? args, CallStack? stack) {
      if (args is null) {
        return null;
      }
      if (args.Cdr is null) {
        return args.Car;
      }

      var joined = new List<Cons>();
      Cons cdr = args;
      while (cdr.Cdr is not null) {
        joined.AddRange(EnsureIsList(cdr.Car).AsCons());
        cdr = (Cons)cdr.Cdr;
      }
      Cons? head = EnsureIsList(cdr.Car);
      foreach (var cons in Enumerable.Reverse(joined)) {
        head = new Cons(cons.Location, cons.Car, head);
      }

      return head;
    }

    private static object? ReverseImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count != 1) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "1");
      }

      return Cons.CreateFromCars(EnsureIsList(args.Car).AsCars().Reverse());
    }

    private static object? StringLengthImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count != 1) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "1");
      }

      string str = EnsureIsString(args.Car);
      return str.Length;
    }

    private static object? StringAppendImpl(Cons? args, CallStack? stack) {
      return string.Join(string.Empty, args.AsCars().Select(x => EnsureIsString(x)));
    }

    private static object? StringToNumberImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count is not 1 and not 2) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "1 or 2");
      }

      var argsEnum = args.AsCars().GetEnumerator();
      string str = EnsureIsString(argsEnum.Take());
      int radix;
      if (args.Count == 2) {
        radix = EnsureIsInt(argsEnum.Take());
      } else {
        radix = 10;
      }
      return Convert.ToInt32(str, radix);
    }

    private static object? NumberToStringImpl(Cons? args, CallStack? stack) {
      if (args is null || args.Count is not 1 and not 2) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "1 or 2");
      }

      var argsEnum = args.AsCars().GetEnumerator();
      double d = ConvertToDouble(argsEnum.Take());
      int radix;
      if (args.Count == 2) {
        radix = EnsureIsInt(argsEnum.Take());
      } else {
        radix = 10;
      }
      if (d % 1 == 0) {
        return Convert.ToString((int)d, radix);
      } else if (radix == 10) {
        return d.ToString();
      } else {
        throw new SchemishException(
            "Converting fractional value to a non-base 10 string not implemented.");
      }
    }

    private static object? DisplayImpl(Interpreter interpreter, Cons? args) {
      if (args is null || args.Count != 1) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "1");
      }
      interpreter.TextualOutputPort.Display(ConvertToString(args.Car));
      return Unspecified.Instance;
    }

    private static object? WriteImpl(Interpreter interpreter, Cons? args) {
      if (args is null || args.Count != 1) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "1");
      }
      interpreter.TextualOutputPort.Display(PrintExpr(args.Car));
      return Unspecified.Instance;
    }

    private static object? NewlineImpl(Interpreter interpreter, Cons? args) {
      if (args is not null) {
        throw SchemishException.IncorrectArity(args.Count, "0");
      }
      interpreter.TextualOutputPort.Newline();
      return Unspecified.Instance;
    }

    private static object? ErrorImpl(Cons? args, CallStack? stack) {
      if (args is null) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "at least 1");
      }

      string message = ConvertToString(args.Car);
      if (args.Count > 1) {
        message += " " + string.Join(' ', args.AsCars().Skip(1));
      }

      throw new SchemishException($"ERROR: {message}");
    }

    private static object? LoadImpl(Interpreter interpreter, Cons? args) {
      if (args is null || args.Count != 1) {
        throw SchemishException.IncorrectArity(args?.Count ?? 0, "1");
      }

      string fileName = EnsureIsString(args.Car);

      using TextReader reader = new StreamReader(interpreter.FileSystemAccessor.OpenRead(fileName));
      return interpreter.EvaluateTextReader(reader, fileName);
    }
  }
}
