using System;

namespace Schemish.Exceptions {
  public class SyntaxErrorException : Exception {
    public SyntaxErrorException() { }

    public SyntaxErrorException(string message)
        : base(message) { }

    public SyntaxErrorException(string message, SourceLocation? location)
        : base($"{location}: {message}") { }

    public SyntaxErrorException(string message, Exception innerException)
        : base(message, innerException) { }

    public SyntaxErrorException(string message, Exception innerException, SourceLocation? location)
        : base($"{location}: {message}", innerException) { }
  }
}