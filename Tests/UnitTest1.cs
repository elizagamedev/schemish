using System.IO;
using System.Reflection;
using Schemish;
using Xunit;

namespace Tests {
  public class UnitTest1 {
    [Fact]
    public void RunTests() {
      var interpreter = new Interpreter(fsAccessor: new ReadOnlyFileSystemAccessor(),
                                        textualOutputPort: new ConsoleTextualOutputPort());
      using var reader = new StreamReader(File.OpenRead(Path.Combine(
          Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "tests.ss")));
      interpreter.EvaluateTextReader(reader, "tests.ss");
    }
  }
}
