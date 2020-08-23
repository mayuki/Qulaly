using System;
using Qulaly.Syntax;

namespace Qulaly
{
    public class QulalyParseException : Exception
    {
        public QulalyParseException(string message)
            : base(message)
        {
        }

        public QulalyParseException(string message, Production production)
            : base($"{message} (at position {production.Position + 1})")
        {
        }
    }
}
