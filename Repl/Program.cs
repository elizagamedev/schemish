using Schemish;
using Schemish.Exceptions;
using static Schemish.Utils;

var interpreter = new Interpreter(
    fsAccessor: new ReadOnlyFileSystemAccessor(),
    textualOutputPort: new ConsoleTextualOutputPort());

// Load any files listed in the command line args.
foreach (string fileName in args) {
  using var fp = File.OpenText(fileName);
  interpreter.EvaluateTextReader(fp, fileName);
}

while (true) {
  string input = ReadLine.Read("> ");
  if (input == ",quit") {
    break;
  }
  try {
    object? eval = interpreter.EvaluateString(input, "<stdin>");
    if (eval is not Unspecified) {
      Console.WriteLine(PrintExpr(eval));
    }
  } catch (Exception e) when (e is RuntimeErrorException or SyntaxErrorException) {
    Console.Error.WriteLine(e.Message);
  }
}
