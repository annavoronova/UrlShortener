using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortener.Exceptions
{
    public class NotExistingUrlException : Exception 
    {
        public NotExistingUrlException() : base() { }
        public NotExistingUrlException(string message) : base(message) { }
        public NotExistingUrlException(string message, Exception innerException) : base(message, innerException) { }
    }
}
