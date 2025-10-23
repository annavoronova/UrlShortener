using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortener.Exceptions
{
    public class DuplicatedSegmentException : Exception 
    {
        public DuplicatedSegmentException() : base() { }
        public DuplicatedSegmentException(string message) : base(message) { }
        public DuplicatedSegmentException(string message, Exception innerException) : base(message, innerException) { }
    }
}
