using System.Collections.Generic;
using System.Linq;

namespace Schemish {
  /// <summary>
  /// Extensions to Schemish classes.
  /// </summary>
  public static class Extensions {
    /// <summary>
    /// Returns an enumerable over self and self's <c>cdr</c> recursively if it is also a
    /// <c>Cons</c>.
    /// </summary>
    /// <param name="this">This. May be null.</param>
    /// <returns>The enumerable.</returns>
    public static IEnumerable<Cons> AsCons(this Cons? @this) {
      if (@this is null) {
        yield break;
      }

      Cons head = @this;
      while (true) {
        yield return head;
        if (head.Cdr is not Cons cdr) {
          break;
        }
        head = cdr;
      }
    }

    /// <summary>
    /// Returns an enumerator over self's <c>car</c> and self's <c>cdr</c>'s <c>car</c> recursively
    /// if self's <c>cdr</c> is also a <c>Cons</c>.
    /// </summary>
    /// <param name="this">This. May be null.</param>
    /// <returns>The enumerable.</returns>
    public static IEnumerable<object?> AsCars(this Cons? @this) {
      return @this.AsCons().Select(x => x.Car);
    }
  }
}
