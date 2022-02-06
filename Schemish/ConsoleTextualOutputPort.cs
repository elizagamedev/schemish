using System;

namespace Schemish {
  public sealed class ConsoleTextualOutputPort : ITextualOutputPort {
    public void Display(string text) {
      Console.Write(text);
    }

    public void Newline() {
      Console.WriteLine();
    }
  }
}
