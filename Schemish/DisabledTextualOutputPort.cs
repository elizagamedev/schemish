namespace Schemish {
  public sealed class DisabledTextualOutputPort : ITextualOutputPort {
    public void Display(string text) { }

    public void Newline() { }
  }
}
