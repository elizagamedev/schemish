using System;

namespace Schemish.Exceptions {
  public sealed class RuntimeErrorException : Exception {
    public RuntimeErrorException() { }

    public RuntimeErrorException(string message)
        : base(message) { }

    public RuntimeErrorException(string message, CallStack? stack)
        : base(FormatMessage(message, stack)) { }

    public RuntimeErrorException(string message, Exception innerException)
        : base(message, innerException) { }

    public RuntimeErrorException(string message, Exception innerException,
                                 CallStack? stack)
        : base(FormatMessage(message, stack), innerException) { }

    private static string FormatMessage(string message, CallStack? stack) {
      if (stack is null) {
        return message;
      } else {
        return $"{message}\n{stack}";
      }
    }
  }
}
