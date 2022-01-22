using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Schemish {
  /// <summary>
  /// Scheme symbol.
  /// </summary>
  /// <remarks>
  /// Symbols are interned so that symbols with the same name are actually of the same symbol object
  /// instance.
  /// </remarks>
  public sealed class Symbol : IEquatable<Symbol> {
    public static readonly ReadOnlyDictionary<string, Symbol> QuotesMap =
        new(new Dictionary<string, Symbol>() {
          { "'", Quote },
          { "`", QuasiQuote },
          { ",", Unquote },
          { ",@", UnquoteSplicing },
        });

    private static Dictionary<string, Symbol>? _interned = null;

    private Symbol(string str) {
      AsString = str;
    }

    public static Symbol If => Intern("if");

    public static Symbol Set => Intern("set!");

    public static Symbol Define => Intern("define");

    public static Symbol Lambda => Intern("lambda");

    public static Symbol Begin => Intern("begin");

    public static Symbol DefineMacro => Intern("define-macro");

    public static Symbol Append => Intern("append");

    public static Symbol Cons => Intern("cons");

    public static Symbol Quote => Intern("quote");

    public static Symbol QuasiQuote => Intern("quasiquote");

    public static Symbol Unquote => Intern("unquote");

    public static Symbol UnquoteSplicing => Intern("unquote-splicing");

    public static Symbol Eof => Intern("#<eof>");

    public string AsString { get; private init; }

    public static bool operator ==(Symbol? lhs, Symbol? rhs) {
      if (lhs is null) {
        if (rhs is null) {
          return true;
        }
        return false;
      }
      return lhs.Equals(rhs);
    }

    public static bool operator !=(Symbol? lhs, Symbol? rhs) {
      if (lhs is null) {
        if (rhs is null) {
          return false;
        }
        return true;
      }
      return !lhs.Equals(rhs);
    }

    public static Symbol Intern(string sym) {
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

    public override string ToString() {
      return AsString;
    }

    public override int GetHashCode() {
      return AsString.GetHashCode();
    }

    public bool Equals(Symbol? other) {
      if (other is null) {
        return false;
      }
      if (ReferenceEquals(this, other)) {
        return true;
      }
      return AsString == other.AsString;
    }

    public override bool Equals(object? obj) {
      return Equals(obj as Symbol);
    }
  }
}
