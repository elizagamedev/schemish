using System;

namespace Schemish.Exceptions {
  /// <summary>
  /// An exception which occurs during evaluation of a Schemish expression.
  /// </summary>
  public sealed class RuntimeErrorException : Exception {
    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeErrorException"/> class.
    /// </summary>
    public RuntimeErrorException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeErrorException"/> class with a specified
    /// error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RuntimeErrorException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeErrorException"/> class with a specified
    /// error message and call stack.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="stack">The call stack leading to the error.</param>
    public RuntimeErrorException(string message, CallStack? stack)
        : base(FormatMessage(message, stack)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeErrorException"/> class with a specified
    /// error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a
    /// <c>null</c> reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public RuntimeErrorException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeErrorException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a
    /// <c>null</c> reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    /// <param name="stack">The call stack leading to the error.</param>
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
