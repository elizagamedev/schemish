using Schemish;
using Schemish.Exceptions;
using static Schemish.Utils;

static object? WriteImpl(Cons? args, List<SourceLocation> stack) {
  if (args is null || args.Count != 1) {
    throw SchemishException.IncorrectArity(args?.Count ?? 0, "1");
  }
  Console.WriteLine(PrintExpr(args.Car));
  return Unspecified.Instance;
}

var interpreter = new Interpreter(environmentInitializers: new Interpreter.CreateSymbolTable[] {
    interpreter => new Dictionary<Symbol, object?> {
      { Symbol.Intern("write"), new NativeProcedure(Symbol.Intern("write"), WriteImpl) },
    },
  }.AsEnumerable());

// var interpreter = new Interpreter();

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
  object? eval = interpreter.EvaluateString(input, "<stdin>");
  if (eval is not Unspecified) {
    Console.WriteLine(PrintExpr(eval));
  }
}
