using System.Globalization;
using System.Text;
using Schemish.Exceptions;

namespace Schemish {
  /// <summary>
  /// Utils interacting with Schemish types.
  /// </summary>
  public static class Utils {
    /// <summary>
    /// Convert the given value to a bool.
    /// </summary>
    /// <remarks>
    /// In Scheme/Schemish, all values except <c>#f</c> evaluate to <c>#t</c>.
    /// </remarks>
    /// <param name="val">The value to convert.</param>
    /// <returns>The value as a bool.</returns>
    public static bool ConvertToBool(object? val) {
      if (val is bool b) {
        return b;
      }
      return true;
    }

    /// <summary>
    /// Convert the given value to a double.
    /// </summary>
    /// <param name="val">The value to convert.</param>
    /// <exception cref="SchemishException">The value cannot be converted to a double.</exception>
    /// <returns>The value as a double.</returns>
    public static double ConvertToDouble(object? val) {
      return val switch {
        int i => i,
        double f => f,
        _ => throw SchemishException.IllegalConversion(val, "number"),
      };
    }

    /// <summary>
    /// Convert the given value to a string.
    /// </summary>
    /// <param name="val">The value to convert.</param>
    /// <returns>The value as a string.</returns>
    public static string ConvertToString(object? val) {
      return val?.ToString() ?? "()";
    }

    /// <summary>
    /// Convert the given value to a Scheme literal string representation, i.e. the output of
    /// <c>(write ...)</c>.
    /// </summary>
    /// <param name="val">The value to convert.</param>
    /// <returns>The value as a Scheme literal string representation.</returns>
    public static string PrintExpr(object? val) {
      return val switch {
        bool boolean => boolean ? "#t" : "#f",
        string s => StringToLiteral(s),
        null => "()",
        _ => val.ToString().AsNotNull(),
      };
    }

    /// <summary>
    /// Convert the given value's type to a string.
    /// </summary>
    /// <param name="val">The value's type to convert.</param>
    /// <returns>The value's type as a string.</returns>
    public static string PrintType(object? val) {
      return val switch {
        bool => "boolean",
        string => "string",
        int or double => "number",
        Symbol => "symbol",
        Cons c => c.IsList ? "list" : "pair",
        ICallable => "procedure",
        null => "null",
        _ => val.GetType().ToString(),
      };
    }

    /// <summary>
    /// Returns the given value as an int if it is an int or integer-double, otherwise throws an
    /// exception.
    /// </summary>
    /// <param name="val">The value to convert.</param>
    /// <exception cref="SchemishException">The value is not an int or integer-double.</exception>
    /// <returns>The value as an int.</returns>
    public static int EnsureIsInt(object? val) {
      if (val is int i) {
        return i;
      }
      if (val is double d && d % 1 == 0) {
        return (int)d;
      }
      throw SchemishException.WrongType(val, "exact integer");
    }

    /// <summary>
    /// Returns the given value as a <see cref="Cons"/> if it is a pair, i.e. a non-list
    /// <see cref="Cons"/>, otherwise throws an exception.
    /// </summary>
    /// <param name="val">The value to convert.</param>
    /// <exception cref="SchemishException">The value is not a pair.</exception>
    /// <returns>The value as a <see cref="Cons"/> pair.</returns>
    public static Cons EnsureIsPair(object? val) {
      if (val is Cons cons) {
        return cons;
      }
      throw SchemishException.WrongType(val, "pair");
    }

    /// <summary>
    /// Returns the given value as a <see cref="Cons"/> if it is a list, otherwise throws an
    /// exception.
    /// </summary>
    /// <param name="val">The value to convert.</param>
    /// <exception cref="SchemishException">The value is not a list.</exception>
    /// <returns>The value as a <see cref="Cons"/> list.</returns>
    public static Cons? EnsureIsList(object? val) {
      if (val is null) {
        return null;
      }
      if (val is Cons cons) {
        if (cons.IsList) {
          return cons;
        }
        object? notCons;
        Cons head = cons;
        while (true) {
          if (head.Cdr is Cons cdr) {
            head = cdr;
          } else {
            notCons = head.Cdr;
            break;
          }
        }
        throw SchemishException.IllegalConversion(notCons, "item in list");
      }
      throw SchemishException.IllegalConversion(val, "list");
    }

    /// <summary>
    /// Returns the given value as a string if it is a string, otherwise throws an exception.
    /// </summary>
    /// <param name="val">The value to convert.</param>
    /// <exception cref="SchemishException">The value is not a string.</exception>
    /// <returns>The value as a string.</returns>
    public static string EnsureIsString(object? val) {
      if (val is string s) {
        return s;
      }
      throw SchemishException.WrongType(val, "string");
    }

    /// <summary>
    /// Returns the given value as an <see cref="ICallable"/> if it is a procedure, otherwise throws
    /// an exception.
    /// </summary>
    /// <param name="val">The value to convert.</param>
    /// <exception cref="SchemishException">The value is not a procedure.</exception>
    /// <returns>The value as a procedure.</returns>
    public static ICallable EnsureIsProc(object? val) {
      if (val is ICallable proc) {
        return proc;
      }
      throw SchemishException.WrongType(val, "procedure");
    }

    /// <summary>
    /// Returns the given value as a <see cref="Symbol"/> if it is a symbol, otherwise throws
    /// an exception.
    /// </summary>
    /// <param name="val">The value to convert.</param>
    /// <exception cref="SchemishException">The value is not a symbol.</exception>
    /// <returns>The value as a symbol.</returns>
    public static Symbol EnsureIsSymbol(object? val) {
      if (val is Symbol symbol) {
        return symbol;
      }
      throw SchemishException.WrongType(val, "symbol");
    }

    /// <summary>
    /// Returns the given value if it is not an instance of <see cref="Unspecified"/>, otherwise
    /// throws an exception.
    /// </summary>
    /// <param name="val">The value to test.</param>
    /// <exception cref="SchemishException">The value is unspecified.</exception>
    /// <returns>The value.</returns>
    public static object? EnsureIsSpecified(object? val) {
      if (val is Unspecified) {
        throw SchemishException.WrongType(val, "unspecified");
      }
      return val;
    }

    // https://stackoverflow.com/a/14087738
    private static string StringToLiteral(string input) {
      var literal = new StringBuilder(input.Length + 2);
      literal.Append("\"");
      foreach (char c in input) {
        switch (c) {
          case '\"':
            literal.Append("\\\"");
            break;
          case '\\':
            literal.Append(@"\\");
            break;
          case '\0':
            literal.Append(@"\0");
            break;
          case '\a':
            literal.Append(@"\a");
            break;
          case '\b':
            literal.Append(@"\b");
            break;
          case '\f':
            literal.Append(@"\f");
            break;
          case '\n':
            literal.Append(@"\n");
            break;
          case '\r':
            literal.Append(@"\r");
            break;
          case '\t':
            literal.Append(@"\t");
            break;
          case '\v':
            literal.Append(@"\v");
            break;
          default:
            // ASCII printable character
            if (c is >= (char)0x20 and <= (char)0x7e) {
              literal.Append(c);
              // As UTF16 escaped character
            } else if (char.GetUnicodeCategory(c) == UnicodeCategory.Control) {
              literal.Append(@"\u");
              literal.Append(((int)c).ToString("x4"));
            } else {
              literal.Append(c);
            }
            break;
        }
      }
      literal.Append("\"");
      return literal.ToString();
    }
  }
}
