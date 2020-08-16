using System;

namespace Qulaly
{
    public class QulalyParseException : Exception
    {
        public QulalyParseException(string message)
            : base(message)
        {
        }
    }
}
