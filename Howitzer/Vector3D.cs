using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howitzer
{
    struct Vector3D<T>
    {
        public T X
        {
            set;
            get;
        }
        public T Y
        {
            set;
            get;
        }
        public T Z
        {
            set;
            get;
        }

        public Vector3D(T x, T y, T z) : this()
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
