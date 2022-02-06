using System;
using System.Collections.Generic;

namespace Schemish {
  /// <summary>
  /// Extensions internal to Schemish.
  /// </summary>
  internal static class InternalExtensions {
    /// <summary>
    /// Moves and takes the next item in an enumerator.
    /// </summary>
    /// <param name="this">This.</param>
    /// <typeparam name="T">The enumerable value type.</typeparam>
    /// <exception cref="InvalidOperationException">Tried to take past the end of an enumerator.
    /// </exception>
    /// <returns>The next item in the enumerator.</returns>
    public static T Take<T>(this IEnumerator<T> @this) {
      if (!@this.MoveNext()) {
        throw new InvalidOperationException("Tried to take past the end of an enumerator.");
      }
      return @this.Current;
    }

    /// <summary>
    /// Throws an exception if self is null, otherwise returns self.
    /// </summary>
    /// <param name="this">This.</param>
    /// <typeparam name="T">The type of this.</typeparam>
    /// <exception cref="ArgumentNullException"><c>this</c> is null.</exception>
    /// <returns>This if not null.</returns>
    public static T AsNotNull<T>(this T? @this) {
      if (@this is null) {
        throw new ArgumentNullException(nameof(@this));
      }
      return @this;
    }
  }
}
