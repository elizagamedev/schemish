using System;

#pragma warning disable SA1402 // FileMayOnlyContainASingleType

namespace Schemish {
  /// <summary>
  /// Exposes standard textual output port to the interpreter, i.e. stdout.
  /// </summary>
  public interface ITextualOutputPort {
    /// <summary>
    /// Display the given text.
    /// </summary>
    /// <param name="text">The text to display.</param>
    void Display(string text);

    /// <summary>
    /// Writes an end-of-line.
    /// </summary>
    void Newline();
  }

  /// <summary>
  /// An implementation of <see cref="ITextualOutputPort"/> that does nothing.
  /// </summary>
  public sealed class DisabledTextualOutputPort : ITextualOutputPort {
    /// <inheritdoc/>
    public void Display(string text) { }

    /// <inheritdoc/>
    public void Newline() { }
  }

  /// <summary>
  /// An implementation of <see cref="ITextualOutputPort"/> that writes to the console.
  /// </summary>
  public sealed class ConsoleTextualOutputPort : ITextualOutputPort {
    /// <inheritdoc/>
    public void Display(string text) {
      Console.Write(text);
    }

    /// <inheritdoc/>
    public void Newline() {
      Console.WriteLine();
    }
  }
}
