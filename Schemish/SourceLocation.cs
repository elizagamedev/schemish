namespace Schemish {
  public sealed record SourceLocation(string FileName, int Line, int Column, string Text) {
    public static SourceLocation Unknown => new("<unknown file>", 0, 0, ";; unknown");

    public override string ToString() {
      return $"{FileName}:{Line}:{Column}: {Text.Trim()}";
    }
  }
}
