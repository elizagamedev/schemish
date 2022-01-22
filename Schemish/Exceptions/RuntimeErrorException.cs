using System;
using System.Collections.Generic;
using System.Linq;

namespace Schemish.Exceptions {
  public class RuntimeErrorException : Exception {
    public RuntimeErrorException() { }

    public RuntimeErrorException(string message)
        : base(message) { }

    public RuntimeErrorException(string message, List<SourceLocation> stack)
        : base(FormatMessage(message, stack)) { }

    public RuntimeErrorException(string message, Exception innerException)
        : base(message, innerException) { }

    public RuntimeErrorException(string message, Exception innerException,
                                 List<SourceLocation> stack)
        : base(FormatMessage(message, stack), innerException) { }

    private static string FormatMessage(string message, List<SourceLocation> stack) {
      return $"{message}\n" + string.Join('\n', Enumerable.Reverse(stack));
    }
  }
}
