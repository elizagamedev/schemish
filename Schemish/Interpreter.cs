using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Schemish.Exceptions;
using static System.Diagnostics.Debug;
using static Schemish.InternalUtils;
using static Schemish.Utils;

namespace Schemish {
  public sealed class Interpreter {
    private readonly Dictionary<Symbol, Procedure> _macroTable;

    /// <summary>
    /// Initializes a new instance of the <see cref="Interpreter"/> class.
    /// </summary>
    /// <param name="environmentInitializers">Array of environment initializers.</param>
    /// <param name="fsAccessor">The file system accessor.</param>
    /// <param name="textualOutputPort">The textual output port interface.</param>
    public Interpreter(IEnumerable<CreateSymbolTable>? environmentInitializers = null,
                       IFileSystemAccessor? fsAccessor = null,
                       ITextualOutputPort? textualOutputPort = null) {
      FileSystemAccessor = fsAccessor ?? new DisabledFileSystemAccessor();
      TextualOutputPort = textualOutputPort ?? new DisabledTextualOutputPort();

      // populate an empty environment for the initializer to potentially work with
      Environment = Environment.CreateEmpty();
      _macroTable = new Dictionary<Symbol, Procedure>();

      environmentInitializers ??= new List<CreateSymbolTable>();
      environmentInitializers = new CreateSymbolTable[] { Builtins.CreateBuiltins }.Concat(
          environmentInitializers);

      foreach (CreateSymbolTable initializer in environmentInitializers) {
        Environment = new Environment(initializer(this), Environment);
      }
    }

    public delegate IDictionary<Symbol, object?> CreateSymbolTable(Interpreter interpreter);

    public Environment Environment { get; private init; }

    public IFileSystemAccessor FileSystemAccessor { get; private init; }

    public ITextualOutputPort TextualOutputPort { get; private init; }

    public object? EvaluateTextReader(TextReader input, string fileName) {
      var port = new TokenParser(input, fileName);
      object? res = Unspecified.Instance;
      while (true) {
        object? sexp = Read(port);
        if (Symbol.Eof.Equals(sexp)) {
          return res;
        }
        object? expanded = Expand(sexp, Environment, _macroTable, isTopLevel: true);
        object? nextRes = Evaluate(expanded, Environment, "#<top>", SourceLocation.Unknown, null);
        if (nextRes is not Unspecified) {
          res = nextRes;
        }
      }
    }

    public object? EvaluateString(string input, string fileName) {
      return EvaluateTextReader(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input))),
                                fileName);
    }

    public void DefineGlobal(Symbol sym, object val) {
      Environment[sym] = val;
    }

    internal static object? Expand(object? expression, Environment env,
                                   Dictionary<Symbol, Procedure> macroTable,
                                   bool isTopLevel = true) {
      if (expression is null) {
        throw new SyntaxErrorException("Unexpected empty list.");
      }
      if (expression is not Cons appliedRef) {
        return expression;
      }
      if (!appliedRef.IsList) {
        throw new SyntaxErrorException("Attempted to expand a pair which was not a list.",
                                       appliedRef.Location);
      }

      object? applied = appliedRef.Car;
      var args = (Cons?)appliedRef.Cdr;

      try {
        if (Symbol.Quote.Equals(applied)) {
          if (args is null || args.Count != 1) {
            throw new SyntaxErrorException("Bad quote form.", appliedRef.Location);
          }

          // Quote expands to itself.
          return appliedRef;
        } else if (Symbol.QuasiQuote.Equals(applied)) {
          if (args is null || args.Count != 1) {
            throw new SyntaxErrorException("Bad quasiquote form.", appliedRef.Location);
          }
          object? expandedQuasiquote = ExpandQuasiquote(args.Car);
          return Expand(expandedQuasiquote, env, macroTable, isTopLevel: false);
        } else if (Symbol.Unquote.Equals(applied)) {
          throw new SyntaxErrorException("unquote not valid outside of quasiquote",
                                         appliedRef.Location);
        } else if (Symbol.UnquoteSplicing.Equals(applied)) {
          throw new SyntaxErrorException("unquote-splicing not valid outside of quasiquote",
                                         appliedRef.Location);
        } else if (Symbol.If.Equals(applied)) {
          if (args is null || args.Count is not 2 and not 3) {
            throw new SyntaxErrorException("Bad if form.", appliedRef.Location);
          }

          var newArgs = args
                        .AsCons()
                        .Select(
                            argRef => new Cons.Floating(
                                argRef.Location,
                                EnsureIsSpecified(Expand(argRef.Car, env, macroTable,
                                                         isTopLevel: false))))
                        .ToList();

          if (args.Count == 2) {
            newArgs.Add(new Cons.Floating(null, Unspecified.Instance));
          }

          return new Cons(appliedRef.Location, Symbol.If, Cons.CreateFromFloating(newArgs));
        } else if (Symbol.Set.Equals(applied)) {
          if (args is null || args.Count != 2) {
            throw new SyntaxErrorException("Bad set! form.", appliedRef.Location);
          }

          var argsRefEnum = args.AsCons().GetEnumerator();
          var nameRef = argsRefEnum.Take();
          EnsureIsSymbol(nameRef.Car);
          var valueRef = argsRefEnum.Take();

          object? expandedValue = EnsureIsSpecified(Expand(valueRef.Car, env, macroTable,
                                                           isTopLevel: false));

          return new Cons(appliedRef.Location, Symbol.Set,
                          new Cons(nameRef.Location, nameRef.Car,
                                   new Cons(valueRef.Location, expandedValue, null)));
        } else if (Symbol.Define.Equals(applied) || Symbol.DefineMacro.Equals(applied)) {
          if (args is null) {
            throw new SyntaxErrorException("Bad define form.", appliedRef.Location);
          }
          var argsRefEnum = args.AsCons().GetEnumerator();
          var firstArgRef = argsRefEnum.Take();

          if (firstArgRef.Car is Cons defNameAndArgs) {
            // defining function: ([define|define-macro] (f arg ...) body)
            if (args.Count < 2 || defNameAndArgs.Count == 0) {
              throw new SyntaxErrorException("Bad define form.", appliedRef.Location);
            }

            EnsureIsList(defNameAndArgs);
            var nameRef = defNameAndArgs;
            EnsureIsSymbol(defNameAndArgs.Car);
            var defArgs = (Cons?)defNameAndArgs.Cdr;

            var body = argsRefEnum.Take();

            var lambda = new Cons(null, Symbol.Lambda,
                                  new Cons(firstArgRef.Location, defArgs, body));

            return Expand(
                new Cons(appliedRef.Location, applied,
                         new Cons(nameRef.Location, nameRef.Car, new Cons(null, lambda, null))),
                env, macroTable, isTopLevel);
          } else {
            // defining variable: ([define|define-macro] id expr)
            if (args.Count != 2) {
              throw new SyntaxErrorException("Bad define form.", appliedRef.Location);
            }

            var nameRef = firstArgRef;
            var name = EnsureIsSymbol(nameRef.Car);
            var valueRef = argsRefEnum.Take();

            object? expandedValue = EnsureIsSpecified(Expand(valueRef.Car, env, macroTable,
                                                             isTopLevel: false));

            if (Symbol.DefineMacro.Equals(applied)) {
              if (!isTopLevel) {
                throw new SyntaxErrorException("Must define macros at the top level.",
                                               appliedRef.Location);
              }
              if (expandedValue is null
                  || Evaluate(expandedValue, env,
                              "#<top>",
                              valueRef.Location,
                              new CallStack("#<top>", appliedRef.Location, null))
                  is not Procedure proc) {
                throw new SyntaxErrorException("Macro body must be a procedure.",
                                               appliedRef.Location);
              }
              macroTable[name] = new Procedure(name, proc.Parameters, proc.Body, proc.Env,
                                               proc.Location);
              return Unspecified.Instance;
            }

            // `define v expr`
            return new Cons(
                appliedRef.Location, Symbol.Define,
                new Cons(nameRef.Location, name, new Cons(valueRef.Location, expandedValue, null)));
          }
        } else if (Symbol.Begin.Equals(applied)) {
          if (args is null) {
            return Unspecified.Instance;
          }

          // Use the same topLevel so that `define-macro` is also allowed in a top-level `begin`.
          var newBody = Cons.CreateFromFloating(
              args.AsCons()
                  .Select(argRef => new Cons.Floating(
                              argRef.Location, Expand(argRef.Car, env, macroTable, isTopLevel)))
                  .Where(argRef => argRef.Car is not Unspecified));
          if (newBody is null) {
            return Unspecified.Instance;
          }

          return new Cons(appliedRef.Location, Symbol.Begin, newBody);
        } else if (Symbol.Lambda.Equals(applied)) {
          if (args is null || args.Count < 2) {
            throw new SyntaxErrorException("Bad lambda form.", appliedRef.Location);
          }

          var argsRefEnum = args.AsCons().GetEnumerator();
          var lambdaArgsRef = argsRefEnum.Take();
          var body = argsRefEnum.Take();

          if (lambdaArgsRef.Car is not null and not Symbol &&
              (lambdaArgsRef.Car is not Cons lambdaArgsCons ||
               lambdaArgsCons.AsCars().Any(v => v is not Symbol))) {
            throw new SyntaxErrorException("Bad lambda form.", appliedRef.Location);
          }

          object? singleBody;
          if (args.Count == 2) {
            // (lambda (...) expr)
            singleBody = body.Car;
          } else {
            // (lambda (...) expr+
            singleBody = new Cons(null, Symbol.Begin, body);
          }

          object? expandedBody = EnsureIsSpecified(Expand(singleBody, env, macroTable,
                                                          isTopLevel: false));

          return new Cons(appliedRef.Location, Symbol.Lambda,
                          new Cons(lambdaArgsRef.Location, lambdaArgsRef.Car,
                                   new Cons(body.Location, expandedBody, null)));
        } else if (applied is Symbol identifier &&
                   macroTable.TryGetValue(identifier, out var procedure)) {
          object? result = procedure.Call(
              args, new CallStack("#<top>", appliedRef.Location, null));
          return Expand(result, env, macroTable, isTopLevel);
        } else {
          var newExpr = Cons.CreateFromFloating(
              appliedRef.AsCons()
                  .Select(argRef => new Cons.Floating(
                              argRef.Location, Expand(argRef.Car, env, macroTable, isTopLevel)))
                  .Where(argRef => argRef.Car is not Unspecified));

          if (newExpr is null) {
            return Unspecified.Instance;
          }
          return newExpr;
        }
      } catch (SchemishException e) {
        throw new SyntaxErrorException("Error while expanding.", e, appliedRef.Location);
      }
    }

    internal static object? Evaluate(object? expr, Environment env, string procedureName,
                                     SourceLocation exprLocation, CallStack? stack) {
      while (true) {
        if (expr is Symbol symbol) {
          if (env.TryGetValue(symbol, out object? val)) {
            return val;
          } else {
            throw new RuntimeErrorException(
                $"Variable `{symbol}' not defined.",
                new CallStack(procedureName, exprLocation, stack));
          }
        }
        if (expr is not Cons appliedRef) {
          return expr;  // is a constant
        }

        var newStack = new CallStack(procedureName,
                                     appliedRef.Location,
                                     stack);
        if (!appliedRef.IsList) {
          throw new RuntimeErrorException("Attempted to evaluate a pair which was not a list.",
                                          newStack);
        }

        object? applied = appliedRef.Car;
        var args = (Cons?)appliedRef.Cdr;

        try {
          if (Symbol.Quote.Equals(applied)) {
            return ((Cons)appliedRef.Cdr.AsNotNull()).Car;
          } else if (Symbol.If.Equals(applied)) {
            Assert(args is not null && args.Count == 3,
                   $"Bad if form during eval. {PrintExpr(expr)}");
            var argsRefEnum = args.AsCons().GetEnumerator();
            var testRef = argsRefEnum.Take();
            var conseqRef = argsRefEnum.Take();
            var altRef = argsRefEnum.Take();
            object? testResult = EnsureIsSpecified(Evaluate(testRef.Car, env, procedureName,
                                                            testRef.Location, stack));
            var newExprRef = ConvertToBool(testResult) ? conseqRef : altRef;
            expr = newExprRef.Car;
            exprLocation = newExprRef.Location;
          } else if (Symbol.Define.Equals(applied)) {
            Assert(args is not null && args.Count == 2,
                   $"Bad define form during eval. {PrintExpr(expr)}");
            var argsRefEnum = args.AsCons().GetEnumerator();
            var variable = (Symbol)argsRefEnum.Take().Car.AsNotNull();
            var valRef = argsRefEnum.Take();
            object? result = EnsureIsSpecified(Evaluate(valRef.Car, env, procedureName,
                                                        valRef.Location, stack));
            if (result is Procedure proc) {
              env[variable] = new Procedure(variable, proc.Parameters, proc.Body, proc.Env,
                                            proc.Location);
            } else {
              env[variable] = result;
            }
            return Unspecified.Instance;
          } else if (Symbol.Set.Equals(applied)) {
            Assert(args is not null && args.Count == 2,
                   $"Bad set! form during eval. {PrintExpr(expr)}");
            var argsRefEnum = args.AsCons().GetEnumerator();
            var name = (Symbol)argsRefEnum.Take().Car.AsNotNull();
            var valRef = argsRefEnum.Take();
            var containingEnv = env.TryFindContainingEnv(name);
            if (containingEnv is null) {
              throw new RuntimeErrorException(
                  $"Symbol {name} not defined in containing environment.", newStack);
            }
            object? result = EnsureIsSpecified(Evaluate(valRef.Car, env, procedureName,
                                                        valRef.Location, stack));
            containingEnv[name] = result;
            return Unspecified.Instance;
          } else if (Symbol.Lambda.Equals(applied)) {
            Assert(args is not null && args.Count == 2,
                   $"Bad lambda form during eval. {PrintExpr(expr)}");
            var argsEnum = args.AsCars().GetEnumerator();
            object? lambdaArgs = argsEnum.Take();
            Assert(lambdaArgs is Symbol or Cons or null,
                   $"Bad lambda form during eval. {PrintExpr(expr)}");
            object? lambdaBody = argsEnum.Take();
            return new Procedure(null, lambdaArgs, lambdaBody, env, appliedRef.Location);
          } else if (Symbol.Begin.Equals(applied)) {
            Assert(args is not null,
                   $"Bad begin form during eval. {PrintExpr(expr)}");
            var child = args;
            while (child.Cdr is not null) {
              Evaluate(child.Car, env, procedureName, child.Location, stack);
              child = (Cons)child.Cdr;
            }
            expr = child.Car;
            exprLocation = child.Location;
          } else {
            // A procedure call.
            object? rawProc = EnsureIsProc(Evaluate(applied, env, procedureName,
                                                    appliedRef.Location, stack));

            var procArgs = Cons.CreateFromCars(
                args
                .AsCons()
                .Select(argRef => EnsureIsSpecified(Evaluate(argRef.Car, env, procedureName,
                                                             argRef.Location, stack))));

            if (rawProc is Procedure proc) {
              // Tail call optimization - instead of evaluating the procedure here which grows the
              // stack by calling Evaluate, we update the `expr` and `env` to be the body
              // and the (params, args), and loop the evaluation from here.
              expr = proc.Body;
              exprLocation = proc.Location;
              env = Environment.FromVariablesAndValues(proc.Parameters, procArgs, proc.Env);
              procedureName = proc.ToString();
              stack = newStack;
            } else if (rawProc is NativeProcedure nativeProc) {
              // Don't copy the stack here, since native procs add themselves to the stack. It's
              // safe to do so, since we return from the function here rather than looping.
              return nativeProc.Call(procArgs, newStack);
            } else {
              throw new InvalidOperationException(
                  $"Unexpected implementation of ICallable: {rawProc.GetType().Name}");
            }
          }
        } catch (SchemishException e) {
          throw new RuntimeErrorException($"Error during evaluation: {e.Message}", e, newStack);
        }
      }
    }

    /// <summary>
    /// Reads an S-expression from the input source.
    /// </summary>
    private static object? Read(TokenParser port) {
      var token = port.NextToken();
      if (token.String is null) {
        return Symbol.Eof;
      }

      static object? ReadNext(TokenParser port, TokenParser.Token token) {
        if (token.String is null) {
          throw new SyntaxErrorException("unexpected EOF", token.Location);
        }

        if (token.String == "(") {
          var list = new List<Cons.Floating>();
          while (true) {
            var nextToken = port.NextToken();
            if (nextToken.String == ")") {
              return Cons.CreateFromFloating(list);
            } else if (nextToken.String == ".") {
              object? tail = ReadNext(port, port.NextToken());
              var endToken = port.NextToken();
              if (endToken.String != ")") {
                throw new SyntaxErrorException("expected ) after dotted pair", token.Location);
              }
              return Cons.CreateFromFloating(list, tail);
            } else {
              list.Add(new Cons.Floating(nextToken.Location, ReadNext(port, nextToken)));
            }
          }
        } else if (token.String == ")") {
          throw new SyntaxErrorException("unexpected )", token.Location);
        } else if (Symbol.QuotesMap.TryGetValue(token.String, out var quote)) {
          var nextToken = port.NextToken();
          object? quoted = ReadNext(port, nextToken);
          return new Cons(token.Location, quote, new Cons(nextToken.Location, quoted, null));
        } else {
          return token.Parse();
        }
      }

      return ReadNext(port, token);
    }

    private static object? ExpandQuasiquote(object? cdr) {
      if (cdr is null || cdr is not Cons valRef) {
        // It's either a quoted nil or a dotted value; just quote it.
        return new Cons(null, Symbol.Quote, new Cons(null, cdr, null));
      }

      object? expanded;

      if (valRef.Car is Cons val) {
        if (Symbol.UnquoteSplicing.Equals(val.Car)) {
          if (val.Count != 2) {
            throw new SyntaxErrorException("Bad unquote-splicing form.", valRef.Location);
          }
          var splicedRef = (Cons)val.Cdr.AsNotNull();
          if (valRef.Cdr is null) {
            return splicedRef.Car;
          } else {
            return new Cons(null, Symbol.Append,
                            new Cons(splicedRef.Location, splicedRef.Car,
                                     new Cons(null, ExpandQuasiquote(valRef.Cdr), null)));
          }
        } else if (Symbol.Unquote.Equals(val.Car)) {
          if (val.Count != 2) {
            throw new SyntaxErrorException("Bad unquote form.", valRef.Location);
          }
          expanded = ((Cons)val.Cdr.AsNotNull()).Car;
        } else {
          // Recurse further into this list.
          expanded = ExpandQuasiquote(val);
        }
      } else {
        // It's not unquoted or a list, so just quote it.
        expanded = new Cons(null, Symbol.Quote, new Cons(valRef.Location, valRef.Car, null));
      }

      return new Cons(null, Symbol.Cons,
                      new Cons(null, expanded, new Cons(null, ExpandQuasiquote(valRef.Cdr), null)));
    }
  }
}
