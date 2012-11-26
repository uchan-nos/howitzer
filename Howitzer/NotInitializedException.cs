using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howitzer
{
    class NotInitializedException : Exception
    {
        public NotInitializedException()
            : base()
        {
        }

        public NotInitializedException(string message)
            : base(message)
        {
        }

        public NotInitializedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public NotInitializedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
