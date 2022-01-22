using System;
using static Schemish.Utils;

namespace Schemish.Exceptions {
  public class SchemishException : Exception {
    public SchemishException() { }

    public SchemishException(string message)
        : base(message) { }

    public SchemishException(string message, Exception innerException)
        : base(message, innerException) { }

    public static SchemishException IncorrectArity(int actual, string desired) {
      return new SchemishException(
          $"Wrong number of arguments {actual}, expecting {desired}.");
    }

    public static SchemishException IllegalConversion(object? actual, string desired) {
      string actualType = PrintType(actual);
      return new SchemishException(
          $"Invalid conversion from {actualType} `{PrintExpr(actual)}' to {desired}.");
    }

    public static SchemishException WrongType(object? actual, string desired) {
      string actualType = PrintType(actual);
      return new SchemishException(
          $"Value `{PrintExpr(actual)}' has incorrect type {actualType} (expected {desired}).");
    }
  }
}
