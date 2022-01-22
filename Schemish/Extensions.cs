using System.Collections.Generic;
using System.Linq;
using static Schemish.InternalUtils;

namespace Schemish {
  public static class Extensions {
    public static IEnumerable<Cons> AsCons(this Cons? @this) {
      if (@this is null) {
        return Enumerable.Empty<Cons>();
      }
      return EnsureIsList(@this).Cast<Cons>();
    }

    public static IEnumerable<object?> AsCars(this Cons? @this) {
      return @this.AsCons().Select(x => x.Car);
    }

    public static IEnumerable<Cons.Floating> AsFloating(this Cons? @this) {
      return @this.AsCons().Select(x => new Cons.Floating(x.Location, x.Car));
    }
  }
}
