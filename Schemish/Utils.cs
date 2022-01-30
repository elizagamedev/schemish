using System.Globalization;
using System.Text;
using Schemish.Exceptions;

namespace Schemish {
  public static class Utils {
    public static bool ConvertToBool(object? val) {
      if (val is bool b) {
        return b;
      }
      return true;
    }

    public static int ConvertToInt(object? val) {
      return val switch {
        int i => i,
        double f => (int)f,
        _ => throw SchemishException.IllegalConversion(val, "number"),
      };
    }

    public static double ConvertToDouble(object? val) {
      return val switch {
        int i => i,
        double f => f,
        _ => throw SchemishException.IllegalConversion(val, "number"),
      };
    }

    public static string ConvertToString(object? val) {
      return val?.ToString() ?? "()";
    }

    public static string PrintExpr(object? x) {
      return x switch {
        bool boolean => boolean ? "#t" : "#f",
        string s => StringToLiteral(s),
        null => "()",
        _ => x.ToString().AsNotNull(),
      };
    }

    public static string PrintType(object? x) {
      return x switch {
        bool => "boolean",
        string => "string",
        int or double => "number",
        Symbol => "symbol",
        Cons c => c.IsList ? "list" : "pair",
        ICallable => "procedure",
        null => "null",
        _ => x.GetType().ToString(),
      };
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
