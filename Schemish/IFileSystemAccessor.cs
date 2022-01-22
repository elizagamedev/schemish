using System.IO;

namespace Schemish {
  /// <summary>
  /// The only interface that the file system should be exposed within a Schemish interpreter.
  /// </summary>
  /// <remarks>
  /// One could implement this interface in a way such that the interpreter can be used to access
  /// "files" in any logical virtual file system. For security purposes, one could also choose to
  /// not implement, say, <see cref="OpenWrite"/> if the interpreter is used in
  /// a way that write does not need to supported. The other (higher) level of protection would be
  /// to not expose any builtin function for writing to the file system.
  /// </remarks>
  public interface IFileSystemAccessor {
    /// <summary>
    /// Opens the path for read.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>the stream to read.</returns>
    Stream OpenRead(string path);

    /// <summary>
    /// Opens the path for write.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>the stream to write.</returns>
    Stream OpenWrite(string path);
  }
}
