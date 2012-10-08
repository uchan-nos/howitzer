using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howitzer
{
    class PixelPoint<T>
    {
        public T X
        {
            private set;
            get;
        }

        public T Y
        {
            private set;
            get;
        }

        public PixelPoint(T x, T y)
        {
            X = x;
            Y = y;
        }
    }
}
