using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
