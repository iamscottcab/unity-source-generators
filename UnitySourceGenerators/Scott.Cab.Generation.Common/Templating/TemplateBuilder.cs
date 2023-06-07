namespace Scott.Cab.Generation.Common
{
    /// <summary>
    /// A minimal wrapper around an <see cref="IndentedStringBuilder"/> to reduce template boilerplate.
    /// </summary>
    public class TemplateBuilder : IndentedStringBuilder
    {
        public void StartBlock()
        {
            AppendLine("{");
            IncrementIndent();
        }

        public void EndBlock()
        {
            DecrementIndent();
            AppendLine("}");
        }

        public void WriteBlock(string value)
        {
            StartBlock();
            AppendLines(value);
            EndBlock();
        }
    }
}
