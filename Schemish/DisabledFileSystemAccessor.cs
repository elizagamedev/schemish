using System;
using System.IO;

namespace Schemish {
  /// <summary>
  /// An implementation of <see cref="IFileSystemAccessor"/> that blocks read/write.
  /// </summary>
  /// <remarks>
  /// This is the default behavior for an interpreter.
  /// </remarks>
  public sealed class DisabledFileSystemAccessor : IFileSystemAccessor {
    public Stream OpenRead(string path) {
      throw new InvalidOperationException(
          "File system access is blocked by DisabledFileSystemAccessor");
    }

    public Stream OpenWrite(string path) {
      throw new InvalidOperationException(
          "File system access is blocked by DisabledFileSystemAccessor");
    }
  }
}
