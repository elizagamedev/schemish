using System;

namespace Schemish.Exceptions {
  /// <summary>
  /// An exception which occurs during parsing or expansion of a Schemish expression.
  /// </summary>
  public sealed class SyntaxErrorException : Exception {
    /// <summary>
    /// Initializes a new instance of the <see cref="SyntaxErrorException"/> class.
    /// </summary>
    public SyntaxErrorException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyntaxErrorException"/> class with a specified
    /// error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public SyntaxErrorException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyntaxErrorException"/> class with a specified
    /// error message and location.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="location">The location in source where the error was encountered.</param>
    public SyntaxErrorException(string message, SourceLocation location)
        : base($"{location}: {message}") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyntaxErrorException"/> class with a specified
    /// error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a
    /// <c>null</c> reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public SyntaxErrorException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyntaxErrorException"/> class with a specified
    /// error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a
    /// <c>null</c> reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    /// <param name="location">The location in source where the error was encountered.</param>
    public SyntaxErrorException(string message, Exception innerException, SourceLocation location)
        : base($"{location}: {message}", innerException) { }
  }
}
