using System.IO;
using System.Text;

namespace Scott.Cab.Generation.Common
{
    // IndentedStringBuilder copied from https://github.com/dotnet/efcore/blob/main/src/EFCore/Infrastructure/IndentedStringBuilder.cs and slimmed down to suit requirements.
    public class IndentedStringBuilder
    {
        private const byte IndentSize = 4;
        private int _indent;
        private bool _indentPending = true;

        private readonly StringBuilder _stringBuilder = new();

        /// <summary>
        ///     Appends the current indent and then the given string to the string being built.
        /// </summary>
        /// <param name="value">The string to append.</param>
        /// <returns>This builder so that additional calls can be chained.</returns>
        public IndentedStringBuilder Append(string value)
        {
            DoIndent();

            _stringBuilder.Append(value);

            return this;
        }

        /// <summary>
        ///     Appends a new line to the string being built.
        /// </summary>
        /// <returns>This builder so that additional calls can be chained.</returns>
        public IndentedStringBuilder AppendLine()
        {
            AppendLine(string.Empty);

            return this;
        }

        /// <summary>
        ///     Appends the current indent, the given string, and a new line to the string being built.
        /// </summary>
        /// <remarks>
        ///     If the given string itself contains a new line, the part of the string after that new line will not be indented.
        /// </remarks>
        /// <param name="value">The string to append.</param>
        /// <returns>This builder so that additional calls can be chained.</returns>
        public IndentedStringBuilder AppendLine(string value)
        {
            if (value.Length != 0)
            {
                DoIndent();
            }

            _stringBuilder.AppendLine(value);

            _indentPending = true;

            return this;
        }

        /// <summary>
        ///     Separates the given string into lines, and then appends each line, prefixed
        ///     by the current indent and followed by a new line, to the string being built.
        /// </summary>
        /// <param name="value">The string to append.</param>
        /// <param name="skipFinalNewline">If <see langword="true" />, then the terminating new line is not added after the last line.</param>
        /// <returns>This builder so that additional calls can be chained.</returns>
        public IndentedStringBuilder AppendLines(string value, bool skipFinalNewline = false)
        {
            using (var reader = new StringReader(value))
            {
                var first = true;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        AppendLine();
                    }

                    if (line.Length != 0)
                    {
                        Append(line);
                    }
                }
            }

            if (!skipFinalNewline)
            {
                AppendLine();
            }

            return this;
        }

        /// <summary>
        ///     Increments the indent.
        /// </summary>
        /// <returns>This builder so that additional calls can be chained.</returns>
        public IndentedStringBuilder IncrementIndent()
        {
            _indent++;

            return this;
        }

        /// <summary>
        ///     Decrements the indent.
        /// </summary>
        /// <returns>This builder so that additional calls can be chained.</returns>
        public IndentedStringBuilder DecrementIndent()
        {
            if (_indent > 0)
            {
                _indent--;
            }

            return this;
        }

        /// <summary>
        ///     Returns the built string.
        /// </summary>
        /// <returns>The built string.</returns>
        public override string ToString()
            => _stringBuilder.ToString();

        private void DoIndent()
        {
            if (_indentPending && _indent > 0)
            {
                _stringBuilder.Append(' ', _indent * IndentSize);
            }

            _indentPending = false;
        }
    }
}
