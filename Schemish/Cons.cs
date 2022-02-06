using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Schemish.Utils;

namespace Schemish {
  /// <summary>
  /// A Scheme cons cell.
  /// </summary>
  public sealed class Cons : IEquatable<Cons> {
    private readonly int _hashCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="Cons"/> class.
    /// <para>As Schemish has no distinct compilation stage and evaluates lists directly, there is
    /// no true distinction between a bit of Schemish code literally written in source, e.g.
    /// <c>(append '(a b) '(c d))</c>, and a list which was evaluted from some expression, e.g.
    /// <c>(a b c d)</c>.</para>
    /// <para>In the former case, Schemish sets the <c>location</c> parameter to the location of the
    /// source. In the latter, this is set to <see cref="SourceLocation.Unknown"/>.</para>
    /// </summary>
    /// <param name="location">The location of the <c>car</c> in source.</param>
    /// <param name="car">The value of <c>car</c>.</param>
    /// <param name="cdr">The value of <c>cdr</c>.</param>
    public Cons(SourceLocation location, object? car, object? cdr) {
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

    /// <summary>
    /// Gets the location of the <c>car</c> in source.
    /// </summary>
    public SourceLocation Location { get; private init; }

    /// <summary>
    /// Gets the value of <c>car</c>.
    /// </summary>
    public object? Car { get; private init; }

    /// <summary>
    /// Gets the value of <c>cdr</c>.
    /// </summary>
    public object? Cdr { get; private init; }

    /// <summary>
    /// Gets the length of the list.
    /// </summary>
    /// <remarks>
    /// This value does not count the cdr of a pair; e.g. The <c>Count</c> of <c>(a . b)</c> returns
    /// 1.
    /// </remarks>
    public int Count { get; private init; }

    /// <summary>
    /// Gets a value indicating whether the <c>Cons</c> is a list, i.e. it's <c>cdr</c> is also a
    /// list. (null is considered a list.)
    /// </summary>
    public bool IsList { get; private init; }

    /// <summary>
    /// Returns true if value-equal according to <see cref="Equals(Cons)"/>.
    /// </summary>
    /// <param name="lhs">The left hand side.</param>
    /// <param name="rhs">The right hand side.</param>
    public static bool operator ==(Cons? lhs, Cons? rhs) {
      if (lhs is null) {
        if (rhs is null) {
          return true;
        }
        return false;
      }
      return lhs.Equals(rhs);
    }

    /// <summary>
    /// Returns true if value-unequal according to <see cref="Equals(Cons)"/>.
    /// </summary>
    /// <param name="lhs">The left hand side.</param>
    /// <param name="rhs">The right hand side.</param>
    public static bool operator !=(Cons? lhs, Cons? rhs) {
      if (lhs is null) {
        if (rhs is null) {
          return false;
        }
        return true;
      }
      return !lhs.Equals(rhs);
    }

    /// <summary>
    /// Creates a new <c>Cons</c> list or pair from the given enumerable of <c>Cons</c>.
    /// </summary>
    /// <param name="enumerable">The <c>Cons</c> which are sourced to build the new <c>Cons</c>.
    /// </param>
    /// <param name="tail">The <c>cdr</c> of the tail-most <c>Cons</c>. If non-null, the result is
    /// not a list.</param>
    /// <returns>The new <c>Cons</c>.</returns>
    public static Cons? CreateFromCons(IEnumerable<Cons> enumerable, object? tail = null) {
      object? head = tail;
      foreach (var cons in Enumerable.Reverse(enumerable)) {
        head = new Cons(cons.Location, cons.Car, head);
      }
      return (Cons?)head;
    }

    /// <summary>
    /// Creates a new <c>Cons</c> list or pair with the given enumerable populating each <c>car</c>.
    /// </summary>
    /// <param name="enumerable">The <c>car</c> values which are sourced to build the new
    /// <c>Cons</c>.</param>
    /// <param name="tail">The <c>cdr</c> of the tail-most <c>Cons</c>. If non-null, the result is
    /// not a list.</param>
    /// <returns>The new <c>Cons</c>.</returns>
    public static Cons? CreateFromCars(IEnumerable<object?> enumerable, object? tail = null) {
      object? head = tail;
      foreach (object? car in Enumerable.Reverse(enumerable)) {
        head = new Cons(SourceLocation.Unknown, car, head);
      }
      return (Cons?)head;
    }

    /// <summary>
    /// Creates a new <c>Cons</c> list or pair from the given enumerable of
    /// <see cref="Floating"/>.
    /// </summary>
    /// <param name="enumerable">the <see cref="Floating"/> cons which are sourced to build the new
    /// <c>Cons</c>.</param>
    /// <param name="tail">the <c>cdr</c> of the tail-most <c>Cons</c>. If non-null, the result is
    /// not a list.</param>
    /// <returns>The new <c>Cons</c>.</returns>
    public static Cons? CreateFromFloating(IEnumerable<Floating> enumerable, object? tail = null) {
      object? head = tail;
      foreach (var cons in Enumerable.Reverse(enumerable)) {
        head = new Cons(cons.Location, cons.Car, head);
      }
      return (Cons?)head;
    }

    /// <inheritdoc/>
    public bool Equals(Cons? other) {
      if (other is null) {
        return false;
      }
      return _hashCode == other._hashCode;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) {
      return Equals(obj as Cons);
    }

    /// <inheritdoc/>
    public override int GetHashCode() {
      return _hashCode;
    }

    /// <inheritdoc/>
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

    /// <summary>
    /// An intermediate type which can be used to construct a new <see cref="Cons"/> from an
    /// enumerable.
    /// </summary>
    public sealed record Floating(SourceLocation Location, object? Car);
  }
}
