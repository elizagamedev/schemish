using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Schemish {
  public sealed class CallStack : IEnumerable, IEnumerable<CallStack> {
    public CallStack(string procedure, SourceLocation location, CallStack? tail) {
      Procedure = procedure;
      Location = location;
      Tail = tail;
    }

    public string Procedure { get; private init; }

    public SourceLocation Location { get; private init; }

    public CallStack? Tail { get; private init; }

    public static CallStack CreateFromNative(string procedure, CallStack? tail) {
      return new CallStack(procedure, new SourceLocation("<native>", 0, 0, string.Empty), tail);
    }

    public IEnumerator<CallStack> GetEnumerator() {
      var head = this;
      while (true) {
        yield return head;
        if (head.Tail is null) {
          break;
        }
        head = head.Tail;
      }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return ((IEnumerable<CallStack>)this).GetEnumerator();
    }

    public override string ToString() {
      return string.Join('\n', this.Select(x => $"  at {x.Procedure} in {x.Location}"));
    }
  }
}
