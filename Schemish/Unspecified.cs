namespace Schemish {
  /// <summary>
  /// Scheme's "unspecified" type, which is "returned" from procedures that do not evaluate to
  /// anything, e.g. <c>(define ...)</c>.
  /// </summary>
  public sealed class Unspecified {
    /// <summary>
    /// The singleton instance of <see cref="Unspecified"/>.
    /// </summary>
    public static readonly Unspecified Instance = new();

    private Unspecified() { }

    /// <inheritdoc/>
    public override string ToString() {
      return "#<unspecified>";
    }
  }
}
