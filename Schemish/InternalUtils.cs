using System.Linq;
using Schemish.Exceptions;

namespace Schemish {
  internal static class InternalUtils {
    public static int EnsureIsInt(object? val) {
      if (val is int i) {
        return i;
      }
      if (val is double d && d % 1 == 0) {
        return (int)d;
      }
      throw SchemishException.WrongType(val, "exact integer");
    }

    public static Cons EnsureIsPair(object? val) {
      if (val is Cons cons) {
        return cons;
      }
      throw SchemishException.WrongType(val, "pair");
    }

    public static Cons? EnsureIsList(object? val) {
      if (val is null) {
        return null;
      }
      if (val is Cons cons) {
        if (cons.IsList) {
          return cons;
        }
        object? notCons = cons.First(x => x is not Cons);
        throw SchemishException.IllegalConversion(notCons, "item in list");
      }
      throw SchemishException.IllegalConversion(val, "list");
    }

    public static string EnsureIsString(object? val) {
      if (val is string s) {
        return s;
      }
      throw SchemishException.WrongType(val, "string");
    }

    public static ICallable EnsureIsProc(object? val) {
      if (val is ICallable proc) {
        return proc;
      }
      throw SchemishException.WrongType(val, "procedure");
    }

    public static Symbol EnsureIsSymbol(object? val) {
      if (val is Symbol symbol) {
        return symbol;
      }
      throw SchemishException.WrongType(val, "symbol");
    }

    public static object? EnsureIsSpecified(object? val) {
      if (val is Unspecified) {
        throw SchemishException.WrongType(val, "unspecified");
      }
      return val;
    }
  }
}
