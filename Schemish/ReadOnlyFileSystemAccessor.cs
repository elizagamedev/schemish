using System;
using System.IO;

namespace Schemish {
  /// <summary>
  /// An implementation of <see cref="IFileSystemAccessor"/> that grants readonly access
  /// to the host file system.
  /// </summary>
  public sealed class ReadOnlyFileSystemAccessor : IFileSystemAccessor {
    public Stream OpenRead(string path) {
      return File.OpenRead(path);
    }

    public Stream OpenWrite(string path) {
      throw new NotSupportedException("Writing to file system is not supported");
    }
  }
}
