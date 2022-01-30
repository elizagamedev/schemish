namespace Schemish {
  public class Unspecified {
    public static readonly Unspecified Instance = new();

    private Unspecified() { }

    public override string ToString() {
      return "#<unspecified>";
    }
  }
}
