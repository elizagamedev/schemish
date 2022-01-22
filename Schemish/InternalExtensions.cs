using System;
using System.Collections.Generic;

namespace Schemish {
  internal static class InternalExtensions {
    public static T Take<T>(this IEnumerator<T> @this) {
      if (!@this.MoveNext()) {
        throw new InvalidOperationException("Tried to take past the end of an enumerator.");
      }
      return @this.Current;
    }

    public static T AsNotNull<T>(this T? @this) {
      if (@this is null) {
        throw new ArgumentNullException(nameof(@this));
      }
      return @this;
    }
  }
}
