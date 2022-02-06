using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Schemish {
  /// <summary>
  /// Scheme symbol.
  /// </summary>
  public sealed class Symbol : IEquatable<Symbol> {
    /// <summary>
    /// Scheme <c>if</c> symbol instance.
    /// </summary>
    public static readonly Symbol If = Intern("if");

    /// <summary>
    /// Scheme <c>set</c> symbol instance.
    /// </summary>
    public static readonly Symbol Set = Intern("set!");

    /// <summary>
    /// Scheme <c>define</c> symbol instance.
    /// </summary>
    public static readonly Symbol Define = Intern("define");

    /// <summary>
    /// Scheme <c>lambda</c> symbol instance.
    /// </summary>
    public static readonly Symbol Lambda = Intern("lambda");

    /// <summary>
    /// Scheme <c>begin</c> symbol instance.
    /// </summary>
    public static readonly Symbol Begin = Intern("begin");

    /// <summary>
    /// Scheme <c>define-macro</c> symbol instance.
    /// </summary>
    public static readonly Symbol DefineMacro = Intern("define-macro");

    /// <summary>
    /// Scheme <c>append</c> symbol instance.
    /// </summary>
    public static readonly Symbol Append = Intern("append");

    /// <summary>
    /// Scheme <c>cons</c> symbol instance.
    /// </summary>
    public static readonly Symbol Cons = Intern("cons");

    /// <summary>
    /// Scheme <c>quote</c> symbol instance.
    /// </summary>
    public static readonly Symbol Quote = Intern("quote");

    /// <summary>
    /// Scheme <c>quasiquote</c> symbol instance.
    /// </summary>
    public static readonly Symbol QuasiQuote = Intern("quasiquote");

    /// <summary>
    /// Scheme <c>unquote</c> symbol instance.
    /// </summary>
    public static readonly Symbol Unquote = Intern("unquote");

    /// <summary>
    /// Scheme <c>unquote-splicing</c> symbol instance.
    /// </summary>
    public static readonly Symbol UnquoteSplicing = Intern("unquote-splicing");

    /// <summary>
    /// Scheme <c>#&lt;eof&gt;</c> symbol instance.
    /// </summary>
    public static readonly Symbol Eof = Intern("#<eof>");

    /// <summary>
    /// A map of quote literals to their corresponding Scheme symbols.
    /// </summary>
    public static readonly ReadOnlyDictionary<string, Symbol> QuotesMap =
        new(new Dictionary<string, Symbol>() {
          { "'", Quote },
          { "`", QuasiQuote },
          { ",", Unquote },
          { ",@", UnquoteSplicing },
        });

    private static Dictionary<string, Symbol>? _interned = null;

    private string _string;

    private Symbol(string str) {
      _string = str;
    }

    /// <summary>
    /// Returns a symbol interned from the given string.
    /// </summary>
    /// <param name="sym">The string representation of the symbol.</param>
    /// <returns>The symbol.</returns>
    public static Symbol Intern(string sym) {
      sym = sym.ToUpperInvariant();
      if (_interned is null) {
        _interned = new Dictionary<string, Symbol>();
      }
      if (_interned.TryGetValue(sym, out Symbol? res)) {
        return res;
      }
      var symbol = new Symbol(sym);
      _interned[sym] = symbol;
      return symbol;
    }

    /// <inheritdoc/>
    public override string ToString() {
      return _string;
    }

    /// <inheritdoc/>
    public override int GetHashCode() {
      return _string.GetHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(Symbol? other) {
      if (other is null) {
        return false;
      }
      if (ReferenceEquals(this, other)) {
        return true;
      }
      return _string == other._string;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) {
      return Equals(obj as Symbol);
    }
  }
}
