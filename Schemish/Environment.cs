using System.Collections.Generic;
using System.Linq;
using Schemish.Exceptions;
using static Schemish.InternalUtils;

namespace Schemish {
  /// <summary>
  /// Tracks the state of an interpreter or a procedure. It supports lexical scoping.
  /// </summary>
  public class Environment {
    private readonly IDictionary<Symbol, object?> _store;

    /// <summary>
    /// The enclosing environment. For top level env, this is null.
    /// </summary>
    private readonly Environment? _outer;

    public Environment(IDictionary<Symbol, object?> env, Environment? outer) {
      _store = env;
      _outer = outer;
    }

    public object? this[Symbol sym] {
      get {
        if (TryGetValue(sym, out object? val)) {
          return val;
        } else {
          throw new SchemishException($"Symbol not defined: {sym}.");
        }
      }

      set => _store[sym] = value;
    }

    public static Environment CreateEmpty() {
      return new Environment(new Dictionary<Symbol, object?>(), outer: null);
    }

    /// <summary>
    /// Attempts to get the value of the symbol. If it's not found in current env, recursively try
    /// the enclosing env.
    /// </summary>
    /// <param name="sym">The the symbol to find.</param>
    /// <param name="val">The value of the symbol to find.</param>
    /// <returns>if the symbol's value could be found.</returns>
    public bool TryGetValue(Symbol sym, out object? val) {
      var env = TryFindContainingEnv(sym);
      if (env is not null) {
        val = env._store[sym];
        return true;
      } else {
        val = null;
        return false;
      }
    }

    /// <summary>
    /// Attempts to find the env that actually defines the symbol.
    /// </summary>
    /// <param name="sym">The symbol to find.</param>
    /// <returns>the env that defines the symbol.</returns>
    public Environment? TryFindContainingEnv(Symbol sym) {
      if (_store.TryGetValue(sym, out _)) {
        return this;
      }

      if (_outer is not null) {
        return _outer.TryFindContainingEnv(sym);
      }

      return null;
    }

    internal static Environment FromVariablesAndValues(object? names, Cons? values,
                                                      Environment outer) {
      Dictionary<Symbol, object?> env;
      if (names is Symbol symbol) {
        env = new Dictionary<Symbol, object?>() { { symbol, values } };
      } else {
        var namesList = EnsureIsList(names);
        int namesCount = namesList?.Count ?? 0;
        int valuesCount = values?.Count ?? 0;
        if (namesCount != valuesCount) {
          throw new SchemishException(
              $"Names and values lists do not match in length ({namesCount} and {valuesCount} respectively).");
        }
        // Ensure the list is made only of symbols.
        var nonSymbols = namesList.AsCars().Where(x => x is not Symbol);
        if (nonSymbols.Any()) {
          throw SchemishException.WrongType(nonSymbols.First(), "symbol");
        }
        env = new Dictionary<Symbol, object?>();
        foreach (var (k, v) in namesList.AsCars().Cast<Symbol>().Zip(values.AsCars(),
                                                                     (k, v) => (k, v))) {
          env[k] = v;
        }
      }
      return new Environment(env, outer);
    }
  }
}
