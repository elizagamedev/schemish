namespace Schemish {
  /// <summary>
  /// A location in a Scheme source file.
  /// </summary>
  public sealed record SourceLocation(string FileName, int Line, int Column, string Text) {
    /// <summary>
    /// An unknown source location.
    /// </summary>
    public static readonly SourceLocation Unknown = new("<unknown file>", 0, 0, ";; unknown");

    /// <summary>
    /// A source location in native code.
    /// </summary>
    public static readonly SourceLocation Native = new("<native>", 0, 0, ";; native code");

    /// <inheritdoc/>
    public override string ToString() {
      return $"{FileName}:{Line}:{Column}: {Text.Trim()}";
    }
  }
}
