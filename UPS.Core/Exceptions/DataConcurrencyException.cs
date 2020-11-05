using System;

namespace UPS.Core.Exceptions
{
    public class DataConcurrencyException : Exception
    {
        public DataConcurrencyException()
        {
        }

        public DataConcurrencyException(string message)
            : base(message)
        {
        }

        public DataConcurrencyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
