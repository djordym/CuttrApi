using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Exceptions
{
    public class UnauthorizedAccessException : Exception
    {
        public UnauthorizedAccessException() { }

        public UnauthorizedAccessException(string message)
            : base(message) { }

        public UnauthorizedAccessException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
