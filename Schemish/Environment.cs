using System.Collections.Generic;
using System.Linq;
using Schemish.Exceptions;
using static Schemish.Utils;

namespace Schemish {
  /// <summary>
  /// Tracks the state of an interpreter or a procedure. It supports lexical scoping.
  /// </summary>
  public sealed class Environment {
    private readonly IDictionary<Symbol, object?> _store;
    private readonly Environment? _outer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Environment"/> class.
    /// </summary>
    /// <param name="env">The backing store of the environment.</param>
    /// <param name="outer">The enclosing environment.</param>
    public Environment(IDictionary<Symbol, object?> env, Environment? outer) {
      _store = env;
      _outer = outer;
    }

    /// <summary>
    /// Gets or sets the value of the specified variable in the environment.
    /// </summary>
    /// <param name="sym">The variable identifier.</param>
    public object? this[Symbol sym] {
      get => _store[sym];
      set => _store[sym] = value;
    }

    /// <summary>
    /// Creates an empty environment.
    /// </summary>
    /// <returns>The empty environment.</returns>
    public static Environment CreateEmpty() {
      return new Environment(new Dictionary<Symbol, object?>(), outer: null);
    }

    /// <summary>
    /// Attempts to get the value of the symbol. If it's not found in current env, recursively try
    /// the enclosing env.
    /// </summary>
    /// <param name="sym">The the symbol to find.</param>
    /// <param name="val">The value of the symbol to find.</param>
    /// <returns>If the symbol's value could be found.</returns>
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
    /// <returns>The env that defines the symbol.</returns>
    public Environment? TryFindContainingEnv(Symbol sym) {
      if (_store.TryGetValue(sym, out _)) {
        return this;
      }

      if (_outer is not null) {
        return _outer.TryFindContainingEnv(sym);
      }

      return null;
    }

    /// <summary>
    /// Creates a new environment from the given variables and values.
    /// </summary>
    /// <param name="names">A <see cref="Symbol"/> or <see cref="Cons"/> list of
    /// <see cref="Symbol"/>s representing the variable names.</param>
    /// <param name="values">A <see cref="Cons"/> list of variable values corresponding to each
    /// name.</param>
    /// <param name="outer">The enclosing environment.</param>
    /// <returns>The new environment.</returns>
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
              $"Names and values lists do not match in length"
              + " ({namesCount} and {valuesCount} respectively).");
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
