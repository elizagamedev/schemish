using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Schemish {
  /// <summary>
  /// The Scheme interpreter's call stack.
  /// </summary>
  public sealed class CallStack : IEnumerable, IEnumerable<CallStack> {
    /// <summary>
    /// Initializes a new instance of the <see cref="CallStack"/> class.
    /// </summary>
    /// <param name="procedure">The string representation of the procedure.</param>
    /// <param name="location">The location of the lambda s-exp which defines the procedure.</param>
    /// <param name="next">The next item in the linked list.</param>
    public CallStack(string procedure, SourceLocation location, CallStack? next) {
      Procedure = procedure;
      Location = location;
      Next = next;
    }

    /// <summary>
    /// Gets the string representation of the procedure.
    /// </summary>
    public string Procedure { get; private init; }

    /// <summary>
    /// Gets the location of the lambda s-exp which defines this function.
    /// </summary>
    public SourceLocation Location { get; private init; }

    /// <summary>
    /// Gets the next item in the linked list.
    /// </summary>
    public CallStack? Next { get; private init; }

    /// <inheritdoc/>
    public IEnumerator<CallStack> GetEnumerator() {
      var head = this;
      while (true) {
        yield return head;
        if (head.Next is null) {
          break;
        }
        head = head.Next;
      }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() {
      return ((IEnumerable<CallStack>)this).GetEnumerator();
    }

    /// <inheritdoc/>
    public override string ToString() {
      return string.Join('\n', this.Select(x => $"  at {x.Procedure} in {x.Location}"));
    }
  }
}
