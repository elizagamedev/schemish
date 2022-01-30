using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Schemish.Utils;

// Disable "must end in Collection"
#pragma warning disable CA1710

namespace Schemish {
  public class Cons : IEnumerable, IEnumerable<object?>, IEquatable<Cons> {
    private readonly int _hashCode;

    public Cons(SourceLocation? location, object? car, object? cdr) {
      Location = location;
      Car = car;
      Cdr = cdr;
      if (cdr is Cons cons) {
        Count = cons.Count + 1;
        IsList = cons.IsList;
      } else {
        Count = 1;
        IsList = cdr is null;
      }
      _hashCode = HashCode.Combine(car, cdr);
    }

    public SourceLocation? Location { get; private init; }

    public object? Car { get; private init; }

    public object? Cdr { get; private init; }

    public int Count { get; private init; }

    public bool IsList { get; private init; }

    public static bool operator ==(Cons? lhs, Cons? rhs) {
      if (lhs is null) {
        if (rhs is null) {
          return true;
        }
        return false;
      }
      return lhs.Equals(rhs);
    }

    public static bool operator !=(Cons? lhs, Cons? rhs) {
      if (lhs is null) {
        if (rhs is null) {
          return false;
        }
        return true;
      }
      return !lhs.Equals(rhs);
    }

    public static Cons? CreateFromCons(IEnumerable<Cons> enumerable, object? tail = null) {
      object? head = tail;
      foreach (var cons in Enumerable.Reverse(enumerable)) {
        head = new Cons(cons.Location, cons.Car, head);
      }
      return (Cons?)head;
    }

    public static Cons? CreateFromCars(IEnumerable<object?> enumerable, object? tail = null) {
      object? head = tail;
      foreach (object? car in Enumerable.Reverse(enumerable)) {
        head = new Cons(null, car, head);
      }
      return (Cons?)head;
    }

    public static Cons? CreateFromFloating(IEnumerable<Floating> enumerable, object? tail = null) {
      object? head = tail;
      foreach (var cons in Enumerable.Reverse(enumerable)) {
        head = new Cons(cons.Location, cons.Car, head);
      }
      return (Cons?)head;
    }

    public IEnumerator<object?> GetEnumerator() {
      object? head = this;
      while (true) {
        yield return head;
        if (head is Cons cons) {
          if (cons.Cdr is null) {
            break;
          }
          head = cons.Cdr;
        } else {
          break;
        }
      }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return ((IEnumerable<object?>)this).GetEnumerator();
    }

    public bool Equals(Cons? other) {
      if (other is null) {
        return false;
      }
      return _hashCode == other._hashCode;
    }

    public override bool Equals(object? obj) {
      return Equals(obj as Cons);
    }

    public override int GetHashCode() {
      return _hashCode;
    }

    public override string ToString() {
      var builder = new StringBuilder("(");
      object? head = this;
      while (head is not null) {
        if (head is Cons cons) {
          if (!ReferenceEquals(head, this)) {
            builder.Append(' ');
          }
          builder.Append(PrintExpr(cons.Car));
          head = cons.Cdr;
        } else {
          builder.Append(" . ");
          builder.Append(PrintExpr(head));
          break;
        }
      }
      builder.Append(')');
      return builder.ToString();
    }

    public record Floating(SourceLocation? Location, object? Car);
  }
}
