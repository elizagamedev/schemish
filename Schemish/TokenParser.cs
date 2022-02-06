using System;
using System.IO;
using System.Text.RegularExpressions;
using Schemish.Exceptions;

namespace Schemish {
  /// <summary>
  /// Scheme source parser.
  /// </summary>
  internal class TokenParser {
    private readonly Regex _tokenizer =
        new(@"^\s*(,@|[('`,)]|""(?:[\\].|[^\\""])*""|;.*|[^\s('""`,;)]*)(.*)");

    private TextReader _file;
    private string? _line;
    private SourceLocation _location;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenParser"/> class.
    /// </summary>
    /// <param name="file">The input stream.</param>
    /// <param name="fileName">The file name used in stack traces for this code.</param>
    public TokenParser(TextReader file, string fileName) {
      _file = file;
      _line = string.Empty;
      _location = new SourceLocation(fileName, 0, 0, string.Empty);
    }

    /// <summary>
    /// Parses and returns the next token from the stream.
    /// </summary>
    /// <returns>The next token.</returns>
    public Token NextToken() {
      while (true) {
        while (_line == string.Empty) {
          _line = _file.ReadLine();
          _location = _location with {
            Line = _location.Line + 1,
            Column = 0,
            Text = _line ?? string.Empty,
          };
        }

        // End of file.
        if (_line is null) {
          return new Token(null, _location);
        }

        var res = _tokenizer.Match(_line);
        string token = res.Groups[1].Value;
        var tokenLocation = _location with {
          Column = _location.Column + res.Groups[1].Index,
        };

        _line = res.Groups[2].Value;
        _location = _location with {
          Column = _location.Column + res.Groups[2].Index,
        };

        if (token == string.Empty) {
          // 1st group is empty. All string falls into 2nd group. This usually means
          // an error in the syntax, e.g., incomplete string "foo
          string tmp = _line;
          _line = string.Empty;  // to continue reading next line

          if (tmp.Trim() != string.Empty) {
            throw new SyntaxErrorException($"Unexpected \"{tmp}\"", _location);
          }
        }

        if (!token.StartsWith(";", StringComparison.Ordinal)) {
          return new Token(token, tokenLocation);
        }
      }
    }

    public record Token(string? String, SourceLocation Location) {
      /// <summary>
      /// Parses the token into a literal value type.
      /// </summary>
      /// <returns>The parsed value.</returns>
      public object Parse() {
        if (String is null) {
          throw new InvalidOperationException();
        } else if (String == "#t") {
          return true;
        } else if (String == "#f") {
          return false;
        } else if (String[0] == '"') {
          return String[1..^1];
        } else if (int.TryParse(String, out int intVal)) {
          return intVal;
        } else if (double.TryParse(String, out double floatVal)) {
          return floatVal;
        } else {
          return Symbol.Intern(String);
        }
      }
    }
  }
}
