using System;
using System.IO;

#pragma warning disable SA1402 // FileMayOnlyContainASingleType

namespace Schemish {
  /// <summary>
  /// Exposes a filesystem to the Schemish interpreter.
  /// </summary>
  /// <remarks>
  /// One could implement this interface in a way such that the interpreter can be used to access
  /// "files" in any logical virtual file system. For security purposes, one could also choose to
  /// not implement, say, <see cref="OpenWrite"/> if the interpreter is used in a way that write
  /// does not need to supported (see <see cref="ReadOnlyFileSystemAccessor"/>). The other (higher)
  /// level of protection would be to not expose any builtin function for writing to the file system
  /// (see <see cref="DisabledFileSystemAccessor"/>).
  /// </remarks>
  public interface IFileSystemAccessor {
    /// <summary>
    /// Opens the path for reading.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The stream to read.</returns>
    Stream OpenRead(string path);

    /// <summary>
    /// Opens the path for writing.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The stream to write.</returns>
    Stream OpenWrite(string path);
  }

  /// <summary>
  /// An implementation of <see cref="IFileSystemAccessor"/> that blocks read/write.
  /// </summary>
  /// <remarks>
  /// This is the default behavior for an interpreter.
  /// </remarks>
  public sealed class DisabledFileSystemAccessor : IFileSystemAccessor {
    /// <inheritdoc/>
    public Stream OpenRead(string path) {
      throw new InvalidOperationException(
          "File system access is blocked by DisabledFileSystemAccessor");
    }

    /// <inheritdoc/>
    public Stream OpenWrite(string path) {
      throw new InvalidOperationException(
          "File system access is blocked by DisabledFileSystemAccessor");
    }
  }

  /// <summary>
  /// An implementation of <see cref="IFileSystemAccessor"/> that grants readonly access to the host
  /// file system.
  /// </summary>
  public sealed class ReadOnlyFileSystemAccessor : IFileSystemAccessor {
    /// <inheritdoc/>
    public Stream OpenRead(string path) {
      return File.OpenRead(path);
    }

    /// <inheritdoc/>
    public Stream OpenWrite(string path) {
      throw new NotSupportedException("Writing to file system is not supported.");
    }
  }
}
